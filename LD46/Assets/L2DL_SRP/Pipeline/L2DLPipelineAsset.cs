using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class L2DLDirectLightSettings
{
    public bool m_enabled = true;
    public L2DLShadowMapCalculationStepAsset m_shadowMapCalculationMethod;
    public float m_directLightShadowMultiplier = 1f;
}

public class L2DLDirectLightData
{
    public bool Enabled { get; }
    public IL2DLShadowMapCalculationStep ShadowMapCalculator { get; }
    public float DirectLightOcclusionMultiplier { get; }

    public L2DLDirectLightData(L2DLDirectLightSettings _settings)
    {
        Enabled = _settings.m_enabled;
        ShadowMapCalculator = _settings?.m_shadowMapCalculationMethod?.CreateStep();
        DirectLightOcclusionMultiplier = _settings.m_directLightShadowMultiplier;
    }
}

[System.Serializable]
public class L2DLIndirectLightSettings
{
    public bool m_enabled = true;
    public L2DLIndirectLightCalculationStepAsset m_indirectLightCalculationMethod;
}

public class L2DLIndirectLightData
{
    public bool Enabled { get; }
    public IL2DLIndirectLightCalculationStep IndirectLightCalculator { get; }

    public L2DLIndirectLightData(L2DLIndirectLightSettings _settings)
    {
        Enabled = _settings.m_enabled;
        IndirectLightCalculator = _settings?.m_indirectLightCalculationMethod?.CreateStep();
    }
}

[System.Serializable]
public class L2DLPipelineSettings
{
    public bool m_dynamicBatching = false;
    public bool m_instancing = true;
    public float m_backgroundDepthFromCamera = 10f;
}

[System.Serializable]
public class L2DLDebugSettings
{
    public bool m_emulateNoComputeShaderSupport = false;
}

[CreateAssetMenu(menuName = "Rendering/My Pipeline")]
public class L2DLPipelineAsset : RenderPipelineAsset
{
    [SerializeField] private L2DLPipelineSettings m_pipelineSettings = null;

    [Header("Direct Light")]
    [SerializeField] private L2DLDirectLightSettings m_directLightSettings = new L2DLDirectLightSettings();

    [Header("Indirect Light")]
    [SerializeField] private L2DLIndirectLightSettings m_indirectLightSettings = new L2DLIndirectLightSettings();

    [Header("Final Presentation")]
    [SerializeField] private L2DLBufferTextures m_textureToView = L2DLBufferTextures.None;

    [Header("Debug")]
    [SerializeField] private L2DLDebugSettings m_debugSettings = new L2DLDebugSettings();

    protected override RenderPipeline CreatePipeline()
    {
        return new L2DLPipeline(m_pipelineSettings, new L2DLDirectLightData(m_directLightSettings), new L2DLIndirectLightData(m_indirectLightSettings), m_textureToView, m_debugSettings);
    }

    override protected void OnValidate()
    {
        m_directLightSettings?.m_shadowMapCalculationMethod?.SetPipelineAsset(this);

        base.OnValidate();

    }

    public void StepAssetChanged()
    {
        OnValidate();
    }
}
