using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

public class L2DLShadowMapCompute : IL2DLShadowMapCalculationStep
{
    public bool OcclusionMapRandomAccess => true;
    public bool OcclusionMapGenerateMips => false;
    public FilterMode OcclusionMapFilterMode => FilterMode.Bilinear;

    ComputeShader m_directionalLightOcclusionTraceCompute = (ComputeShader)Resources.Load("Shaders/Direct Light/Occlusion Trace Compute/DirectionalOcclusionMapTrace");
    ComputeShader m_pointLightOcclusionTraceCompute = (ComputeShader)Resources.Load("Shaders/Direct Light/Occlusion Trace Compute/PointOcclusionMapTrace");
    ComputeShader m_spotLightOcclusionTraceCompute = (ComputeShader)Resources.Load("Shaders/Direct Light/Occlusion Trace Compute/SpotOcclusionMapTrace");

    int m_directLightOcclusionMapId = Shader.PropertyToID("_L2DLDirectLightOcclusionMap");
    int m_directLightShadowMapId = Shader.PropertyToID("_L2DLDirectLightShadowMap");
    int m_directLightDepthTextureId = Shader.PropertyToID("_L2DLDirectLightDepthTexture");

    // --------------------------------------------------------------------
    public void CalculateShadowMap(ScriptableRenderContext _context, CommandBuffer _buffer, L2DLDirectionalLight _directionalLight, L2DLDirectLightData _data, RenderTargetIdentifier i_occlusionMap, RenderTargetIdentifier o_shadowMap)
    {
        if(!L2DLRenderHelpers.SupportsComputeShaders())
        {
            return;
        }

        ReloadComputeShaders();

        int occlusionTraceComputeKernel = m_directionalLightOcclusionTraceCompute.FindKernel("DirectionalLightOcclusionTrace");
        SetupCommonComputeShaderProperties(_buffer, m_directionalLightOcclusionTraceCompute, occlusionTraceComputeKernel, _directionalLight, _data);
        _buffer.SetComputeFloatParam(m_directionalLightOcclusionTraceCompute, "_WorldDistPerStep", _directionalLight.Height / (int)_directionalLight.ShadowMapSize);
        _buffer.DispatchCompute(m_directionalLightOcclusionTraceCompute, occlusionTraceComputeKernel, (int)_directionalLight.ShadowMapSize / 64, 1, 1);
        L2DLRenderHelpers.ExecuteBuffer(_context, _buffer);
    }

    // --------------------------------------------------------------------
    public void CalculateShadowMap(ScriptableRenderContext _context, CommandBuffer _buffer, L2DLPointLight _pointLight, L2DLDirectLightData _data, RenderTargetIdentifier i_occlusionMap, RenderTargetIdentifier o_shadowMap)
    {
        if (!L2DLRenderHelpers.SupportsComputeShaders())
        {
            return;
        }

        ReloadComputeShaders();

        int occlusionTraceComputeKernel = m_pointLightOcclusionTraceCompute.FindKernel("PointLightOcclusionTrace");
        SetupCommonComputeShaderProperties(_buffer, m_pointLightOcclusionTraceCompute, occlusionTraceComputeKernel, _pointLight, _data);
        _buffer.SetComputeFloatParam(m_pointLightOcclusionTraceCompute, "_TextureSizeHalf", (int)_pointLight.ShadowMapSize / 2);
        _buffer.SetComputeFloatParam(m_pointLightOcclusionTraceCompute, "_TexturePercentagePerPixel", 1f / (int)_pointLight.ShadowMapSize);
        _buffer.SetComputeFloatParam(m_pointLightOcclusionTraceCompute, "_WorldDistPerStep", _pointLight.Range * 2f / (int)_pointLight.ShadowMapSize);
        _buffer.DispatchCompute(m_pointLightOcclusionTraceCompute, occlusionTraceComputeKernel, (int)_pointLight.ShadowMapSize / 16, 4, 1);
        L2DLRenderHelpers.ExecuteBuffer(_context, _buffer);
    }

    // --------------------------------------------------------------------
    public void CalculateShadowMap(ScriptableRenderContext _context, CommandBuffer _buffer, L2DLSpotLight _spotLight, L2DLDirectLightData _data, RenderTargetIdentifier i_occlusionMap, RenderTargetIdentifier o_shadowMap)
    {
        if (!L2DLRenderHelpers.SupportsComputeShaders())
        {
            return;
        }

        ReloadComputeShaders();

        int occlusionTraceComputeKernel = m_spotLightOcclusionTraceCompute.FindKernel("SpotLightOcclusionTrace");
        SetupCommonComputeShaderProperties(_buffer, m_spotLightOcclusionTraceCompute, occlusionTraceComputeKernel, _spotLight, _data);
        _buffer.SetComputeFloatParam(m_spotLightOcclusionTraceCompute, "_WorldDistPerStep", _spotLight.Range / (int)_spotLight.ShadowMapSize);
        _buffer.DispatchCompute(m_spotLightOcclusionTraceCompute, occlusionTraceComputeKernel, (int)_spotLight.ShadowMapSize / 64, 1, 1);
        L2DLRenderHelpers.ExecuteBuffer(_context, _buffer);
    }

    // --------------------------------------------------------------------
    void SetupCommonComputeShaderProperties(CommandBuffer buffer, ComputeShader shader, int kernel, IL2DLDirectLight light, L2DLDirectLightData _data)
    {
        buffer.SetComputeTextureParam(shader, kernel, "Occlusion", m_directLightOcclusionMapId);
        buffer.SetComputeTextureParam(shader, kernel, "Shadow", m_directLightShadowMapId);
        buffer.SetComputeIntParam(shader, "_TextureSize", (int)light.ShadowMapSize);
        buffer.SetComputeFloatParam(shader, "_DirectLightOcclusionMultiplier", _data.DirectLightOcclusionMultiplier);
    }

    // --------------------------------------------------------------------
    [Conditional("UNITY_EDITOR")]
    private void ReloadComputeShaders()
    {
        m_directionalLightOcclusionTraceCompute = (ComputeShader)Resources.Load("Compute Shaders/Direct Light/Occlusion Trace Compute/DirectionalOcclusionMapTrace");
        m_pointLightOcclusionTraceCompute = (ComputeShader)Resources.Load("Compute Shaders/Direct Light/Occlusion Trace Compute/PointOcclusionMapTrace");
        m_spotLightOcclusionTraceCompute = (ComputeShader)Resources.Load("Compute Shaders/Direct Light/Occlusion Trace Compute/SpotOcclusionMapTrace");
    }
}
