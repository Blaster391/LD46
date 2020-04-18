using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public interface IL2DLIndirectLightCalculationStep
{
    List<LPVIterationData> LPVIterationData { get; }

    void CalculateIndirectLight(
        ScriptableRenderContext _context, 
        CommandBuffer _buffer, 
        Camera _camera, 
        RenderTargetIdentifier i_emissive, 
        RenderTargetIdentifier i_occlusion, 
        RenderTargetIdentifier o_totalLight
        );
}
