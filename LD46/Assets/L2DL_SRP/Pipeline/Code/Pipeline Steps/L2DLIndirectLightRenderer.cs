using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

/* 
 * L2DLDirectLightRenderer
 * 
 * This class handles rendering indirect light
 * 
 * It will:
 * - Figure out which lights can affect the scene
 * - For each of them
 *      - Generate culling results
 *      - Render those object's occlusion data into an occlusion map texture
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

public partial class L2DLIndirectLightRenderer
{
    ScriptableRenderContext m_context;
    Camera m_camera;

    L2DLIndirectLightData m_indirectLightData;

    CommandBuffer m_indirectLightRenderingBuffer = new CommandBuffer() { name = "Indirect Light Rendering" };
    CommandBuffer m_clearBuffer = new CommandBuffer() { name = "Clear" };
    
    Shader m_directionalLightPassShader = Shader.Find("L2DL/DirectionalLightPass");
    Material m_directionalLightPassMaterial;

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

    // --------------------------------------------------------------------
    public L2DLIndirectLightRenderer(L2DLIndirectLightData _indirectLightData)
    {
        m_indirectLightData = _indirectLightData;
    }

    // --------------------------------------------------------------------
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        if(!m_indirectLightData.Enabled)
        {
            return;
        }

        m_context = context;
        m_camera = camera;
        
        // Encapsulate in frame debugger
        L2DLRenderHelpers.BeginSample(m_context, m_indirectLightRenderingBuffer);
        {
            m_indirectLightData.IndirectLightCalculator?.CalculateIndirectLight(
                m_context, 
                m_indirectLightRenderingBuffer,
                m_camera,
                L2DLPipelineData.s_cameraEmissionTextureId,
                L2DLPipelineData.s_cameraOcclusionTextureId,
                L2DLPipelineData.s_cameraIndirectLightResultTextureId);
        }
        L2DLRenderHelpers.EndSample(m_context, m_indirectLightRenderingBuffer);
    }
}
