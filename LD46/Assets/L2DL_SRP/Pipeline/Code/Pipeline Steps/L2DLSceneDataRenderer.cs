using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

/* 
 * This class handles the initial step of the L2DL pipeline, rendering the scene into the different buffers
 * The class is responsible for producing the buffer data that can then be used by any other steps to create the games lighting
 * The buffer textures this produces will be a constant and core part of how L2DL operates.
 * 
 * Individual objects, knowing the order of the buffers as render targets, can write into these buffers in whatever way they like
 * using their own shaders.
 * 
 * The resulting buffers are:
 *  - Color (R, G, B - color, A)
 *  - Emission (R, G, B define lighting strength, A)
 *  - Occlusion (R, G, B define the light that should be ABSORBED, A)
 *  - Additional (R - Reflectivity used to reflect direct light and convert it to emissive light sources, G - N/A, B - N/A, A)
 *  
 */

public partial class L2DLSceneDataRenderer
{
    ScriptableRenderContext m_context;
    Camera m_camera;

    ScriptableCullingParameters m_cullingParameters;
    CullingResults m_cullingResults;

    CommandBuffer m_sceneDataRenderBuffer = new CommandBuffer() { name = "Scene Data Renderer" };
    CommandBuffer m_clearTexturebuffer = new CommandBuffer() { name = "Clear Texture" };

    // --------------------------------------------------------------------
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        m_context = context;
        m_camera = camera;
        
        // Sample to encapsulate all used buffers under one heading in the frame debugger
        L2DLRenderHelpers.BeginSample(m_context, m_sceneDataRenderBuffer);
        {
            m_context.SetupCameraProperties(m_camera);

            // MUST go before culling so that in world UI is setup pre-cull
            L2DLRenderHelpers.PrepareCameraForSceneWindow(m_camera);

            if (!m_camera.TryGetCullingParameters(out m_cullingParameters))
            {
                return;
            }
            m_cullingResults = m_context.Cull(ref m_cullingParameters);

            //Clear the main output textures
            m_clearTexturebuffer.SetRenderTarget(new RenderTargetIdentifier[]
            {
                L2DLPipelineData.s_cameraColorTextureId
            },
            L2DLPipelineData.s_cameraDepthTextureId);
            if (m_camera.cameraType == CameraType.SceneView)
            {
                // The scene m_camera's settings aren't always set to clear things, it's quite odd so hard code it to clear
                m_clearTexturebuffer.ClearRenderTarget(true, true, m_camera.backgroundColor.linear);
            }
            else
            {
                m_clearTexturebuffer.ClearRenderTarget(m_camera.clearFlags <= CameraClearFlags.Depth, m_camera.clearFlags == CameraClearFlags.Color, m_camera.clearFlags == CameraClearFlags.Color ? m_camera.backgroundColor.linear : Color.clear);
            }

            //Clear the buffer textures
            m_clearTexturebuffer.SetRenderTarget(new RenderTargetIdentifier[]
            {
                L2DLPipelineData.s_cameraOcclusionTextureId,
                L2DLPipelineData.s_cameraEmissionTextureId,
                L2DLPipelineData.s_cameraAdditionalDataTextureId
            },
            L2DLPipelineData.s_cameraDepthTextureId);
            m_clearTexturebuffer.ClearRenderTarget(false, true, Color.clear);
            L2DLRenderHelpers.ExecuteBuffer(m_context, m_clearTexturebuffer);

            // Set render targets for rendering
            m_sceneDataRenderBuffer.SetRenderTarget(new RenderTargetIdentifier[]
            {
                L2DLPipelineData.s_cameraColorTextureId,
                L2DLPipelineData.s_cameraOcclusionTextureId,
                L2DLPipelineData.s_cameraEmissionTextureId,
                L2DLPipelineData.s_cameraAdditionalDataTextureId
            },
            L2DLPipelineData.s_cameraDepthTextureId);
            L2DLRenderHelpers.ExecuteBuffer(m_context, m_sceneDataRenderBuffer);

            L2DLRenderHelpers.DrawAllRenderers(m_context, m_cullingResults);
        }
        L2DLRenderHelpers.EndSample(m_context, m_sceneDataRenderBuffer);

        // Editor Only
        L2DLRenderHelpers.DrawUnsupportedShaders(m_context, m_cullingResults);
    }
}
