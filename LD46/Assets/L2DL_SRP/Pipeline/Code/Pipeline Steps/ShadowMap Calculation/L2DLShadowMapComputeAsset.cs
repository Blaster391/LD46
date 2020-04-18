using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/L2DL/Direct Light/Shadow Map Calculation/Compute")]
public class L2DLShadowMapComputeAsset : L2DLShadowMapCalculationStepAsset
{
    // Any configurable parameters go here and are passed in to the constructor

    public override IL2DLShadowMapCalculationStep CreateStep()
    {
        return new L2DLShadowMapCompute();
    }
}
