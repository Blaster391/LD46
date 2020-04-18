using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/L2DL/Direct Light/Shadow Map Calculation/Render")]
public class L2DLShadowMapRenderAsset : L2DLShadowMapCalculationStepAsset
{
    // Any configurable parameters go here and are passed in to the constructor
    [SerializeField] [Range(0, 16)] private int m_samplesPerPixel = 5;

    public override IL2DLShadowMapCalculationStep CreateStep()
    {
        return new L2DLShadowMapRender(m_samplesPerPixel);
    }
}
