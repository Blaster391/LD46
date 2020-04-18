using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class LPVIterationData
{
    public int MipLevel;
    public int Iterations;
    public int TotalStartIteration;
    public bool InjectAmbientLight;
}

[CreateAssetMenu(menuName = "Rendering/L2DL/Indirect Light Calculation/LPV")]
public class L2DLIndirectLightCalculationLPVAsset : L2DLIndirectLightCalculationStepAsset
{
    // Any configurable parameters go here and are passed in to the constructor
    [SerializeField] private List<LPVIterationData> m_lpvIterationsData = null;

    public override IL2DLIndirectLightCalculationStep CreateStep()
    {
        return new L2DLIndirectLightCalculationLPV(m_lpvIterationsData);
    }
}
