using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class L2DLPipelineChangeableStepAsset : ScriptableObject
{
    private L2DLPipelineAsset m_pipelineAsset = null;

    public void SetPipelineAsset(L2DLPipelineAsset _pipelineAsset)
    {
        m_pipelineAsset = _pipelineAsset;
        // Might need some way to 'unsub' this but fine for now
    }

    public void OnValidate()
    {
        m_pipelineAsset?.StepAssetChanged();
    }
}

[System.Serializable]
public abstract class L2DLPipelineChangeableStepAsset<TStep> : L2DLPipelineChangeableStepAsset
{
    public abstract TStep CreateStep();
}
