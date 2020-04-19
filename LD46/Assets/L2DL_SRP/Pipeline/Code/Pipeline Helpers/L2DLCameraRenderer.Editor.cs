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

#if UNITY_EDITOR 
        
    // --------------------------------------------------------------------
    partial void DrawGizmos()
    {
        if(Handles.ShouldRenderGizmos())
        {
            m_context.DrawGizmos(m_camera, GizmoSubset.PreImageEffects);
            m_context.DrawGizmos(m_camera, GizmoSubset.PostImageEffects);
        }
    }

#elif DEVELOPMENT_BUILD
    
    //string SampleName => m_bufferName;

#endif // UNITY_EDITOR
}
