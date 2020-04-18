using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

partial class L2DLCameraRenderer
{
    partial void DrawGizmos();
    partial void PrepareBuffer();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        
    // --------------------------------------------------------------------
    partial void DrawGizmos()
    {
        if(Handles.ShouldRenderGizmos())
        {
            m_context.DrawGizmos(m_camera, GizmoSubset.PreImageEffects);
            m_context.DrawGizmos(m_camera, GizmoSubset.PostImageEffects);
        }
    }

#else 
    
    string SampleName => m_bufferName;

#endif // UNITY_EDITOR
}
