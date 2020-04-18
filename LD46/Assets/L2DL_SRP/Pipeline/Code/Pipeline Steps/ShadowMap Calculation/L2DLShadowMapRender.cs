using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

public class L2DLShadowMapRender : IL2DLShadowMapCalculationStep
{
    // From asset
    private int m_samplesPerPixel;

    private static Material s_directionalShadowMapRenderMaterial;
    private static Material DirectionalShadowMapRenderMaterial
    {
        get
        {
            if (s_directionalShadowMapRenderMaterial == null)
            {
                s_directionalShadowMapRenderMaterial = new Material(Shader.Find("L2DL/Direct Light/Shadow Map Rendering/DirectionalLightShadowMapRender"));
            }
            return s_directionalShadowMapRenderMaterial;
        }
    }

    public bool OcclusionMapRandomAccess => false;
    public bool OcclusionMapGenerateMips => true;
    public FilterMode OcclusionMapFilterMode => FilterMode.Trilinear;

    public L2DLShadowMapRender(int _samplesPerPixel)
    {
        m_samplesPerPixel = _samplesPerPixel;
    }

    public void CalculateShadowMap(ScriptableRenderContext _context, CommandBuffer _buffer, L2DLDirectionalLight _directionalLight, L2DLDirectLightData _data, RenderTargetIdentifier i_occlusionMap, RenderTargetIdentifier o_shadowMap)
    {
        // Here we run the code that calculates the shadow map using standard rendering...
        _buffer.SetGlobalTexture("_OcclusionMap", i_occlusionMap);
        _buffer.SetGlobalInt("_SamplesPerPixel", m_samplesPerPixel);
        _buffer.SetGlobalFloat("_WorldDistPerStep", _directionalLight.Height / m_samplesPerPixel);
        _buffer.Blit(i_occlusionMap, o_shadowMap, DirectionalShadowMapRenderMaterial);
        L2DLRenderHelpers.ExecuteBuffer(_context, _buffer);
    }
    
    public void CalculateShadowMap(ScriptableRenderContext _context, CommandBuffer _buffer, L2DLPointLight _pointLight, L2DLDirectLightData _data, RenderTargetIdentifier i_occlusionMap, RenderTargetIdentifier o_shadowMap)
    {
        throw new System.NotImplementedException();
    }

    public void CalculateShadowMap(ScriptableRenderContext _context, CommandBuffer _buffer, L2DLSpotLight _spotLight, L2DLDirectLightData _data, RenderTargetIdentifier i_occlusionMap, RenderTargetIdentifier o_shadowMap)
    {
        throw new System.NotImplementedException();
    }
}
