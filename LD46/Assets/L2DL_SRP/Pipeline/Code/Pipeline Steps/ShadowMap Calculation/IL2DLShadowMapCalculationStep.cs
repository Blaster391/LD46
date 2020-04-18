using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public interface IL2DLShadowMapCalculationStep
{
    void CalculateShadowMap(ScriptableRenderContext _context, CommandBuffer _buffer, L2DLDirectionalLight _directionalLight, L2DLDirectLightData _data, RenderTargetIdentifier i_occlusionMap, RenderTargetIdentifier o_shadowMap);
    void CalculateShadowMap(ScriptableRenderContext _context, CommandBuffer _buffer, L2DLPointLight _pointLight, L2DLDirectLightData _data, RenderTargetIdentifier i_occlusionMap, RenderTargetIdentifier o_shadowMap);
    void CalculateShadowMap(ScriptableRenderContext _context, CommandBuffer _buffer, L2DLSpotLight _spotLight, L2DLDirectLightData _data, RenderTargetIdentifier i_occlusionMap, RenderTargetIdentifier o_shadowMap);
    
    // Occlusion map requirements as the textures are temp requested before shadow map calculation
    bool OcclusionMapRandomAccess { get; }
    bool OcclusionMapGenerateMips { get; }
    FilterMode OcclusionMapFilterMode { get; }
}
