using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

public enum L2DLBufferTextures
{
    None,
    Colour,
    Emission,
    Occlusion,
    AdditionalData,
    Depth
}

public class L2DLDirectLights
{
    public List<L2DLDirectionalLight> m_directionalLights = new List<L2DLDirectionalLight>();
    public List<L2DLPointLight> m_pointLights = new List<L2DLPointLight>();
    public List<L2DLSpotLight> m_spotLights = new List<L2DLSpotLight>();

    public void Clear()
    {
        m_directionalLights.Clear();
        m_pointLights.Clear();
        m_spotLights.Clear();
    }
}

public class L2DLPipeline : RenderPipeline
{
    private L2DLCameraRenderer m_cameraRenderer;

    private L2DLDirectLights m_directLights = new L2DLDirectLights();
    
    public L2DLPipeline
    (
        L2DLPipelineSettings pipelineSettings,
        L2DLDirectLightData directLightData,
        L2DLIndirectLightData indirectLightData,
        L2DLBufferTextures textureToView,
        L2DLDebugSettings debugSettings
    )
    {
        // Setup some of the pipeline settings
        L2DLPipelineData.s_backgroundDepthFromCamera = pipelineSettings.m_backgroundDepthFromCamera;

        L2DLPipelineData.m_drawingSettingsOpaque = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), L2DLPipelineData.SortingSettingsOpaque)
        {
            enableDynamicBatching = pipelineSettings.m_dynamicBatching,
            enableInstancing = pipelineSettings.m_instancing
        };

        L2DLPipelineData.m_drawingSettingsTransparent = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), L2DLPipelineData.SortingSettingsTransparent)
        {
            enableDynamicBatching = pipelineSettings.m_dynamicBatching,
            enableInstancing = pipelineSettings.m_instancing
        };

        L2DLPipelineData.s_emulateNoComputeShaderSupport = debugSettings.m_emulateNoComputeShaderSupport;

        m_cameraRenderer = new L2DLCameraRenderer(directLightData, indirectLightData, textureToView);
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        // 2D Lights will have associated cameras because amazingly it's impossible to cull without a camera so here I'll need to filter light cameras out from non-light cameras
        // I can then handle the light cameras seperately to generate their occlusion / shadow maps and let each actual camera reuse them
        m_directLights.Clear();

        // When in the scene view it doesn't pass in all the cameras active in the scene
        // so we need to get them manually. The performance hit is only in scene view so won't hit in-game
        Camera[] directLightPotentialCameras = cameras;
#if UNITY_EDITOR
        directLightPotentialCameras = Object.FindObjectsOfType<Camera>();
        Camera mainCamera = null;
#endif

        foreach (Camera camera in directLightPotentialCameras)
        {
            if (camera.tag == "L2DLDirectLight")
            {
                L2DLDirectionalLight directionalLight = camera.gameObject.GetComponent<L2DLDirectionalLight>();
                if (directionalLight != null)
                {
                    m_directLights.m_directionalLights.Add(directionalLight);
                    continue;
                }

                L2DLPointLight pointLight = camera.gameObject.GetComponent<L2DLPointLight>();
                if(pointLight != null)
                {
                    m_directLights.m_pointLights.Add(pointLight);
                    continue;
                }

                L2DLSpotLight spotLight = camera.gameObject.GetComponent<L2DLSpotLight>();
                if (spotLight != null)
                {
                    m_directLights.m_spotLights.Add(spotLight);
                    continue;
                }
            }
#if UNITY_EDITOR
            else if (camera.tag == "MainCamera")
            {
                mainCamera = camera;
            }
#endif
        }

        // Only pass non-direct light rendering cameras to the pipeline
        // Direct light cameras can be accessed via the associated direct light component
        foreach (Camera camera in cameras)
        {
            if (camera.tag != "L2DLDirectLight")
            {
#if UNITY_EDITOR
                // The scene camera is in a strange place, and usually doesn't render depth
                // so we'll find the main camera, and bring it in line with that, 
                // shift the clip plants, and render to the depth texture
                camera.depthTextureMode = DepthTextureMode.Depth;
                Vector3 cameraPosition = camera.transform.position;
                cameraPosition.z = mainCamera.transform.position.z;
                camera.transform.position = cameraPosition;
                camera.nearClipPlane = mainCamera.nearClipPlane;
                camera.farClipPlane = mainCamera.farClipPlane;
#endif
                m_cameraRenderer.Render(context, camera, m_directLights);
            }
        }
    }
}
