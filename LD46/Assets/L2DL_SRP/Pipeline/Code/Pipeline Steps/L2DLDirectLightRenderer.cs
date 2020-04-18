using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

/* 
 * L2DLDirectLightRenderer
 * 
 * This class handles rendering direct lights
 * 
 * It will:
 * - Figure out which lights can affect the scene
 * - For each of them
 *      - Generate culling results
 *      - Render those object's occlusion data into an occlusion map texture
 *      (TODO: We can use the main scene textures if the light's source is on screen - look at old project for reference)
 *      - Process that texture to trace the occlusion and generate a 2D 'shadow map'
 * - Then pass over the scene and process for each light
 *      - Figure out if the given pixel is within the lights affected area
 *      - Convert from screen/world space into the light's shadow map
 *      - Sample the shadow map and accumulate the lighting contribution
 * 
 * I should be able to follow how the LWRP calculates stuff for its different lights, then
 * I just need to pass in, for each light, a matrix that goes from the current pixel to the 
 * shadow map UV so I can sample it
 */

public partial class L2DLDirectLightRenderer
{
    ScriptableRenderContext m_context;
    Camera m_camera;

    L2DLDirectLightData m_directLightData;

    ScriptableCullingParameters m_cullingParameters;
    CullingResults m_cullingResults;

    // Buffers for a tidy Frame Debugger
    CommandBuffer m_directLightRenderingBuffer = new CommandBuffer() { name = "Direct Light Rendering" };
    CommandBuffer m_clearBuffer = new CommandBuffer() { name = "Clear" };

    CommandBuffer m_directionalLightsBuffer = new CommandBuffer() { name = "Directional Lights" };
    CommandBuffer m_directionalLightRenderingBuffer = new CommandBuffer() { name = "Directional Light Rendering" };

    CommandBuffer m_pointLightsBuffer = new CommandBuffer() { name = "Point Lights" };
    CommandBuffer m_pointLightRenderingBuffer = new CommandBuffer() { name = "Point Light Rendering" };

    CommandBuffer m_spotLightsBuffer = new CommandBuffer() { name = "Spot Lights" };
    CommandBuffer m_spotLightRenderingBuffer = new CommandBuffer() { name = "Spot Light Rendering" };

    CommandBuffer m_occlusionMapRenderingBuffer = new CommandBuffer() { name = "Occlusion Map Rendering" };
    CommandBuffer m_textureHandlingBuffer = new CommandBuffer() { name = "Texture Handling" };

    int m_directLightOcclusionMapId = Shader.PropertyToID("_L2DLDirectLightOcclusionMap");
    int m_directLightShadowMapId = Shader.PropertyToID("_L2DLDirectLightShadowMap");
    int m_directLightDepthTextureId = Shader.PropertyToID("_L2DLDirectLightDepthTexture");

    int m_directLightDummyTextureId = Shader.PropertyToID("_L2DLDirectLightDummyTexture");

    Shader m_directionalLightPassShader = Shader.Find("L2DL/DirectionalLightPass");
    Shader m_pointLightPassShader = Shader.Find("L2DL/PointLightPass");
    Shader m_spotLightPassShader = Shader.Find("L2DL/SpotLightPass");

    Material m_directionalLightPassMaterial;
    Material m_pointLightPassMaterial;
    Material m_spotLightPassMaterial;

    Material DirectionalLightPassMaterial
    {
        get
        {
            if (m_directionalLightPassMaterial == null)
            {
                m_directionalLightPassMaterial = new Material(m_directionalLightPassShader);
            }
            return m_directionalLightPassMaterial; 
        }
    }
    Material PointLightPassMaterial
    {
        get
        {
            if(m_pointLightPassMaterial == null)
            {
                m_pointLightPassMaterial = new Material(m_pointLightPassShader);
            }
            return m_pointLightPassMaterial;
        }
    }
    Material SpotLightPassMaterial
    {
        get
        {
            if (m_spotLightPassMaterial == null)
            {
                m_spotLightPassMaterial = new Material(m_spotLightPassShader);
            }
            return m_spotLightPassMaterial;
        }
    }

    // --------------------------------------------------------------------
    public L2DLDirectLightRenderer(L2DLDirectLightData _directLightData)
    {
        m_directLightData = _directLightData;
    }

    // --------------------------------------------------------------------
    public void Render(ScriptableRenderContext context, Camera camera, L2DLDirectLights directLights)
    {
        if(!m_directLightData.Enabled)
        {
            return;
        }

        m_context = context;
        m_camera = camera;

        // Encapsulate in frame debugger
        L2DLRenderHelpers.BeginSample(m_context, m_directLightRenderingBuffer);
        {
            CoreUtils.SetRenderTarget(m_clearBuffer, L2DLPipelineData.s_cameraDirectLightResultTextureId, L2DLPipelineData.s_cameraDepthTextureId, ClearFlag.Color);
            L2DLRenderHelpers.ExecuteBuffer(m_context, m_clearBuffer);
            
            // Directional Lights
            L2DLRenderHelpers.BeginSample(m_context, m_directionalLightsBuffer);
            foreach (L2DLDirectionalLight directionalLight in directLights.m_directionalLights)
            {
                GetOcclusionTextures((int)directionalLight.ShadowMapSize); // Probably don't need to do this every time?
                RenderOcclusionMapForLight(directionalLight);
                RunDirectionalLightOcclusionMapTrace(directionalLight);
                RunDirectionalLightRendering(directionalLight);
                ReleaseOcclusionTextures();
            }
            L2DLRenderHelpers.EndSample(m_context, m_directionalLightsBuffer);
            
            // Point Lights 
            L2DLRenderHelpers.BeginSample(m_context, m_pointLightsBuffer);
            foreach (L2DLPointLight pointLight in directLights.m_pointLights)
            {
                GetOcclusionTextures((int)pointLight.ShadowMapSize);
                RenderOcclusionMapForLight(pointLight);
                RunPointLightOcclusionMapTrace(pointLight);
                RunPointLightRendering(pointLight);
                ReleaseOcclusionTextures();
            }
            L2DLRenderHelpers.EndSample(m_context, m_pointLightsBuffer);

            // Spot Lights 
            L2DLRenderHelpers.BeginSample(m_context, m_spotLightsBuffer);
            foreach (L2DLSpotLight spotLight in directLights.m_spotLights)
            {
                GetOcclusionTextures((int)spotLight.ShadowMapSize);
                RenderOcclusionMapForLight(spotLight);
                RunSpotLightOcclusionMapTrace(spotLight);
                RunSpotLightRendering(spotLight);
                ReleaseOcclusionTextures();
            }
            L2DLRenderHelpers.EndSample(m_context, m_spotLightsBuffer);

            // Reset
            m_context.SetupCameraProperties(m_camera);
        }
        L2DLRenderHelpers.EndSample(m_context, m_directLightRenderingBuffer);
    }

    // --------------------------------------------------------------------
    void RenderOcclusionMapForLight(IL2DLDirectLight directLight)
    {
        L2DLRenderHelpers.BeginSample(m_context, m_occlusionMapRenderingBuffer);
        {
            m_context.SetupCameraProperties(directLight.ShadowCamera);

            if (!directLight.ShadowCamera.TryGetCullingParameters(out m_cullingParameters))
            {
                return;
            }
            m_cullingResults = m_context.Cull(ref m_cullingParameters);

            CoreUtils.SetRenderTarget(m_clearBuffer, new RenderTargetIdentifier[] { m_directLightDummyTextureId, m_directLightOcclusionMapId }, m_directLightDepthTextureId, ClearFlag.All);
            L2DLRenderHelpers.ExecuteBuffer(m_context, m_clearBuffer);

            L2DLRenderHelpers.DrawAllRenderers(m_context, m_cullingResults);
        }
        L2DLRenderHelpers.EndSample(m_context, m_occlusionMapRenderingBuffer);
    }

    // --------------------------------------------------------------------
    void RunDirectionalLightOcclusionMapTrace(L2DLDirectionalLight directionalLight)
    {
        m_directLightData.ShadowMapCalculator.CalculateShadowMap(m_context, m_directionalLightRenderingBuffer, directionalLight, m_directLightData, m_directLightOcclusionMapId, m_directLightShadowMapId);
    }
    void RunPointLightOcclusionMapTrace(L2DLPointLight pointLight)
    {
        m_directLightData.ShadowMapCalculator.CalculateShadowMap(m_context, m_pointLightRenderingBuffer, pointLight, m_directLightData, m_directLightOcclusionMapId, m_directLightShadowMapId);
    }
    void RunSpotLightOcclusionMapTrace(L2DLSpotLight spotLight)
    {
        m_directLightData.ShadowMapCalculator.CalculateShadowMap(m_context, m_spotLightRenderingBuffer, spotLight, m_directLightData, m_directLightOcclusionMapId, m_directLightShadowMapId);
    }

    // Light
    // --------------------------------------------------------------------
    void SetupCommonLightRenderingProperties(CommandBuffer buffer, IL2DLDirectLight light)
    {
        // Buffer textures
        buffer.SetGlobalTexture("_L2DLAdditionalDataTexture", L2DLPipelineData.s_cameraAdditionalDataTextureId); // TODO: Make a map of string to ID so I don't have to put the string everywhere
        
        // Values
        buffer.SetGlobalMatrix("_CameraToWorld", Matrix4x4.Inverse(GL.GetGPUProjectionMatrix(m_camera.projectionMatrix, true) * m_camera.worldToCameraMatrix));
        buffer.SetGlobalMatrix("_WorldToCamera", GL.GetGPUProjectionMatrix(m_camera.projectionMatrix, true) * m_camera.worldToCameraMatrix);
        buffer.SetGlobalMatrix("_WorldToShadow", GL.GetGPUProjectionMatrix(light.ShadowCamera.projectionMatrix, true) * light.ShadowCamera.worldToCameraMatrix);
        buffer.SetGlobalVector("_lightColor", light.Color);
        buffer.SetGlobalFloat("_lightIntensity", light.Intensity);
        buffer.SetGlobalFloat("_backgroundDepthFromCameraNormalised",  L2DLPipelineData.s_backgroundDepthFromCamera / m_camera.farClipPlane);
    }

    // --------------------------------------------------------------------
    void RunDirectionalLightRendering(L2DLDirectionalLight directionalLight)
    {
        SetupCommonLightRenderingProperties(m_directionalLightRenderingBuffer, directionalLight);
        
        m_directionalLightRenderingBuffer.SetRenderTarget(new RenderTargetIdentifier[] { L2DLPipelineData.s_cameraDirectLightResultTextureId, L2DLPipelineData.s_cameraEmissionTextureId }, L2DLPipelineData.s_cameraFakeDepthTextureId);
        m_directionalLightRenderingBuffer.Blit(null, BuiltinRenderTextureType.CurrentActive, DirectionalLightPassMaterial);
        
        L2DLRenderHelpers.ExecuteBuffer(m_context, m_directionalLightRenderingBuffer);
    }

    // --------------------------------------------------------------------
    void RunPointLightRendering(L2DLPointLight pointLight)
    {
        SetupCommonLightRenderingProperties(m_pointLightRenderingBuffer, pointLight);
        m_pointLightRenderingBuffer.SetGlobalFloat("_lightAttenuation", 1f / Mathf.Max(pointLight.Range * pointLight.Range, 0.00001f));
        m_pointLightRenderingBuffer.SetGlobalVector("_lightPosition", pointLight.transform.position);

        m_pointLightRenderingBuffer.SetRenderTarget(new RenderTargetIdentifier[] { L2DLPipelineData.s_cameraDirectLightResultTextureId, L2DLPipelineData.s_cameraEmissionTextureId }, L2DLPipelineData.s_cameraFakeDepthTextureId);
        m_pointLightRenderingBuffer.Blit(null, BuiltinRenderTextureType.CurrentActive, PointLightPassMaterial);

        L2DLRenderHelpers.ExecuteBuffer(m_context, m_pointLightRenderingBuffer);
    }

    // --------------------------------------------------------------------
    void RunSpotLightRendering(L2DLSpotLight spotLight)
    {
        SetupCommonLightRenderingProperties(m_spotLightRenderingBuffer, spotLight);
        m_spotLightRenderingBuffer.SetGlobalFloat("_lightAttenuation", 1f / Mathf.Max(spotLight.Range * spotLight.Range, 0.00001f));
        m_spotLightRenderingBuffer.SetGlobalVector("_lightPosition", spotLight.transform.position + spotLight.transform.up * spotLight.Range / 2f);
        m_spotLightRenderingBuffer.SetGlobalVector("_lightDirection", -spotLight.transform.up);

        float outerRad = Mathf.Deg2Rad * 0.5f * spotLight.Angle;
        float outerCos = Mathf.Cos(outerRad);
        float outerTan = Mathf.Tan(outerRad);
        float innerCos = Mathf.Cos(Mathf.Atan((46f / 64f) * outerTan));
        float angleRange = Mathf.Max(innerCos - outerCos, 0.00001f);
        float spotFade = 1f / angleRange;
        m_spotLightRenderingBuffer.SetGlobalVector("_lightFade", new Vector2(spotFade, -outerCos * spotFade));
        
        m_spotLightRenderingBuffer.SetRenderTarget(new RenderTargetIdentifier[] { L2DLPipelineData.s_cameraDirectLightResultTextureId, L2DLPipelineData.s_cameraEmissionTextureId }, L2DLPipelineData.s_cameraFakeDepthTextureId);
        m_spotLightRenderingBuffer.Blit(null, BuiltinRenderTextureType.CurrentActive, SpotLightPassMaterial);
        L2DLRenderHelpers.ExecuteBuffer(m_context, m_spotLightRenderingBuffer);
    }

    // Textures
    // --------------------------------------------------------------------
    void GetOcclusionTextures(int _size)
    {
        RenderTextureDescriptor mapDescriptor = new RenderTextureDescriptor(_size, _size, RenderTextureFormat.ARGBHalf)
        {
            useMipMap = m_directLightData.ShadowMapCalculator.OcclusionMapGenerateMips,
            autoGenerateMips = m_directLightData.ShadowMapCalculator.OcclusionMapGenerateMips,
            enableRandomWrite = m_directLightData.ShadowMapCalculator.OcclusionMapRandomAccess,
        };

        m_textureHandlingBuffer.GetTemporaryRT(m_directLightOcclusionMapId, mapDescriptor, m_directLightData.ShadowMapCalculator.OcclusionMapFilterMode);
        m_textureHandlingBuffer.GetTemporaryRT(m_directLightShadowMapId, mapDescriptor, m_directLightData.ShadowMapCalculator.OcclusionMapFilterMode);
        m_textureHandlingBuffer.GetTemporaryRT(m_directLightDummyTextureId, _size, _size, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
        m_textureHandlingBuffer.GetTemporaryRT(m_directLightDepthTextureId, _size, _size, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
        L2DLRenderHelpers.ExecuteBuffer(m_context, m_textureHandlingBuffer);
    }

    // --------------------------------------------------------------------
    void ReleaseOcclusionTextures()
    {
        m_textureHandlingBuffer.ReleaseTemporaryRT(m_directLightOcclusionMapId);
        m_textureHandlingBuffer.ReleaseTemporaryRT(m_directLightShadowMapId);
        m_textureHandlingBuffer.ReleaseTemporaryRT(m_directLightDummyTextureId);
        m_textureHandlingBuffer.ReleaseTemporaryRT(m_directLightDepthTextureId);
        L2DLRenderHelpers.ExecuteBuffer(m_context, m_textureHandlingBuffer);
    }
}
