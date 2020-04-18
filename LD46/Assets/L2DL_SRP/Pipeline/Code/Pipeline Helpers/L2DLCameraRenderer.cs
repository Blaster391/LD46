using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

public partial class L2DLCameraRenderer
{
    ScriptableRenderContext m_context;
    Camera m_camera;
    
    CommandBuffer m_presentFinalImageBuffer = new CommandBuffer() { name = "Present Final Image" };
    CommandBuffer m_textureHandlerBuffer = new CommandBuffer() { name = "Texture Handling" };

    RenderTextureDescriptor m_bufferTextureDescriptor = new RenderTextureDescriptor()
    {
        autoGenerateMips = false,
        colorFormat = RenderTextureFormat.ARGBHalf,
        width = 0,
        height = 0,
        volumeDepth = 1,
        msaaSamples = 1,
        dimension = TextureDimension.Tex2D
    };
    
    L2DLBufferTextures m_textureToView;

    L2DLSceneDataRenderer m_sceneDataRenderer = new L2DLSceneDataRenderer();
    L2DLDirectLightRenderer m_directLightRenderer;
    L2DLIndirectLightRenderer m_indirectLightRenderer;
    L2DLIndirectLightData m_indirectLightData;
    
    // --------------------------------------------------------------------
    // Initilisation
    // --------------------------------------------------------------------

    // --------------------------------------------------------------------
    public L2DLCameraRenderer
    (
        L2DLDirectLightData directLightData,
        L2DLIndirectLightData indirectLightData,
        L2DLBufferTextures textureToView
    )
    {
        GraphicsSettings.lightsUseLinearIntensity = true;
        
        m_textureToView = textureToView;

        m_directLightRenderer = new L2DLDirectLightRenderer(directLightData);
        m_indirectLightRenderer = new L2DLIndirectLightRenderer(indirectLightData);
        m_indirectLightData = indirectLightData;
    }

    // --------------------------------------------------------------------
    // Render Loop
    // --------------------------------------------------------------------

    // --------------------------------------------------------------------
    public void Render(ScriptableRenderContext context, Camera camera, L2DLDirectLights directLights)
    {
        m_context = context;
        m_camera = camera;

        RenderSetup();

        m_sceneDataRenderer.Render(m_context, m_camera);
        
        m_directLightRenderer.Render(m_context, m_camera, directLights);
        m_indirectLightRenderer.Render(m_context, m_camera);

        RenderStep_PresentFinalImage();
        
        DrawGizmos();

        m_context.Submit();

        RenderShutdown();
    }

    // --------------------------------------------------------------------
    // Pipeline Sections
    // --------------------------------------------------------------------

    // --------------------------------------------------------------------
    void RenderSetup()
    {
        GetTextures();
    }

    // --------------------------------------------------------------------
    void RenderShutdown()
    {
        ReleaseTextures();
    }

    // --------------------------------------------------------------------
    void RenderStep_PresentFinalImage()
    {
        // Push the RT we've written on to the camera target
        switch (m_textureToView)
        {
            case L2DLBufferTextures.None:
                m_presentFinalImageBuffer.SetGlobalTexture("_Color", L2DLPipelineData.s_cameraColorTextureId);
                m_presentFinalImageBuffer.SetGlobalTexture("_DirectLight", L2DLPipelineData.s_cameraDirectLightResultTextureId);
                m_presentFinalImageBuffer.SetGlobalTexture("_IndirectLight", L2DLPipelineData.s_cameraIndirectLightResultTextureId);
                List<LPVIterationData> iterationData = m_indirectLightData.IndirectLightCalculator.LPVIterationData;
                m_presentFinalImageBuffer.SetGlobalInt("_IndirectLightMip", iterationData[iterationData.Count - 1].MipLevel);
                m_presentFinalImageBuffer.Blit(null, BuiltinRenderTextureType.CameraTarget, L2DLPipelineData.CombineMaterial);
            break;

            case L2DLBufferTextures.Colour:
                m_presentFinalImageBuffer.SetGlobalTexture("_MainTex", L2DLPipelineData.s_cameraColorTextureId);
                m_presentFinalImageBuffer.Blit(L2DLPipelineData.s_cameraColorTextureId, BuiltinRenderTextureType.CameraTarget, L2DLPipelineData.BlitMaterial);
                break;

            case L2DLBufferTextures.Emission:
                m_presentFinalImageBuffer.SetGlobalTexture("_MainTex", L2DLPipelineData.s_cameraEmissionTextureId);
                m_presentFinalImageBuffer.Blit(L2DLPipelineData.s_cameraEmissionTextureId, BuiltinRenderTextureType.CameraTarget, L2DLPipelineData.BlitMaterial);
                break;

            case L2DLBufferTextures.Occlusion:
                m_presentFinalImageBuffer.SetGlobalTexture("_MainTex", L2DLPipelineData.s_cameraOcclusionTextureId);
                m_presentFinalImageBuffer.Blit(L2DLPipelineData.s_cameraOcclusionTextureId, BuiltinRenderTextureType.CameraTarget, L2DLPipelineData.BlitMaterial);
                break;

            case L2DLBufferTextures.AdditionalData:
                m_presentFinalImageBuffer.SetGlobalTexture("_MainTex", L2DLPipelineData.s_cameraAdditionalDataTextureId);
                m_presentFinalImageBuffer.Blit(L2DLPipelineData.s_cameraAdditionalDataTextureId, BuiltinRenderTextureType.CameraTarget, L2DLPipelineData.BlitMaterial);
                break;

            case L2DLBufferTextures.Depth:
                m_presentFinalImageBuffer.SetGlobalTexture("_MainTex", L2DLPipelineData.s_cameraDepthTextureId);
                m_presentFinalImageBuffer.Blit(L2DLPipelineData.s_cameraDepthTextureId, BuiltinRenderTextureType.CameraTarget, L2DLPipelineData.BlitMaterial);
                break;
        }

        L2DLRenderHelpers.ExecuteBuffer(m_context, m_presentFinalImageBuffer);
    }

    // Textures need to be gotten at the start of the cameras rendering and held on to until the end so that all steps can access them
    // --------------------------------------------------------------------
    void GetTextures()
    {
        RenderTextureDescriptor bufferTextureDescriptor = new RenderTextureDescriptor(m_camera.pixelWidth, m_camera.pixelHeight, RenderTextureFormat.ARGBHalf, 0)
        {
            enableRandomWrite = true,
            useMipMap = true,
            autoGenerateMips = false,
        };

        RenderTextureDescriptor bufferDepthTextureDescriptor = new RenderTextureDescriptor(m_camera.pixelWidth, m_camera.pixelHeight, RenderTextureFormat.Depth, 16);

        m_textureHandlerBuffer.GetTemporaryRT(L2DLPipelineData.s_cameraColorTextureId, bufferTextureDescriptor, FilterMode.Point);
        m_textureHandlerBuffer.GetTemporaryRT(L2DLPipelineData.s_cameraEmissionTextureId, bufferTextureDescriptor, FilterMode.Point);
        m_textureHandlerBuffer.GetTemporaryRT(L2DLPipelineData.s_cameraOcclusionTextureId, bufferTextureDescriptor, FilterMode.Point);
        m_textureHandlerBuffer.GetTemporaryRT(L2DLPipelineData.s_cameraAdditionalDataTextureId, bufferTextureDescriptor, FilterMode.Point);
        m_textureHandlerBuffer.GetTemporaryRT(L2DLPipelineData.s_cameraDirectLightResultTextureId, bufferTextureDescriptor, FilterMode.Point);
        m_textureHandlerBuffer.GetTemporaryRT(L2DLPipelineData.s_cameraIndirectLightResultTextureId, bufferTextureDescriptor, FilterMode.Point);
        
        m_textureHandlerBuffer.GetTemporaryRT(L2DLPipelineData.s_cameraDepthTextureId, bufferDepthTextureDescriptor, FilterMode.Point);
        m_textureHandlerBuffer.GetTemporaryRT(L2DLPipelineData.s_cameraFakeDepthTextureId, bufferDepthTextureDescriptor, FilterMode.Point);

        L2DLRenderHelpers.ExecuteBuffer(m_context, m_textureHandlerBuffer);
    }

    // --------------------------------------------------------------------
    void ReleaseTextures()
    {
        m_textureHandlerBuffer.ReleaseTemporaryRT(L2DLPipelineData.s_cameraColorTextureId);
        m_textureHandlerBuffer.ReleaseTemporaryRT(L2DLPipelineData.s_cameraEmissionTextureId);
        m_textureHandlerBuffer.ReleaseTemporaryRT(L2DLPipelineData.s_cameraOcclusionTextureId);
        m_textureHandlerBuffer.ReleaseTemporaryRT(L2DLPipelineData.s_cameraAdditionalDataTextureId);
        m_textureHandlerBuffer.ReleaseTemporaryRT(L2DLPipelineData.s_cameraDepthTextureId);
        m_textureHandlerBuffer.ReleaseTemporaryRT(L2DLPipelineData.s_cameraFakeDepthTextureId);
        m_textureHandlerBuffer.ReleaseTemporaryRT(L2DLPipelineData.s_cameraDirectLightResultTextureId);
        m_textureHandlerBuffer.ReleaseTemporaryRT(L2DLPipelineData.s_cameraIndirectLightResultTextureId);
        L2DLRenderHelpers.ExecuteBuffer(m_context, m_textureHandlerBuffer);
    }
}
