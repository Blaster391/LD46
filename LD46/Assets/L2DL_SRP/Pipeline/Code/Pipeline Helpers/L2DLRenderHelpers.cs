using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

public static class L2DLRenderHelpers
{
    private static Material s_errorMaterial;

    // --------------------------------------------------------------------
    public static void DrawAllRenderers(ScriptableRenderContext _context, CullingResults _cullingResults)
    {
        _context.DrawRenderers(_cullingResults, ref L2DLPipelineData.m_drawingSettingsOpaque, ref L2DLPipelineData.m_filteringSettingsOpaque);
        _context.DrawRenderers(_cullingResults, ref L2DLPipelineData.m_drawingSettingsTransparent, ref L2DLPipelineData.m_filteringSettingsTransparent);
    }

    // --------------------------------------------------------------------
    public static void BeginSample(ScriptableRenderContext _context, CommandBuffer _buffer)
    {
        _buffer.BeginSample(_buffer.name);
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    // --------------------------------------------------------------------
    public static void EndSample(ScriptableRenderContext _context, CommandBuffer _buffer)
    {
        _buffer.EndSample(_buffer.name);
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    // --------------------------------------------------------------------
    public static void ExecuteBuffer(ScriptableRenderContext _context, CommandBuffer _buffer)
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    // --------------------------------------------------------------------
    [Conditional("UNITY_EDITOR")]
    public static void DrawUnsupportedShaders(ScriptableRenderContext _context, CullingResults _cullingResults)
    {
        if (s_errorMaterial == null)
        {
            s_errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader")) { hideFlags = HideFlags.HideAndDontSave };
        }

        DrawingSettings drawingSettingsError = new DrawingSettings(new ShaderTagId("ForwardBase"), new SortingSettings())
        {
            overrideMaterial = s_errorMaterial,
        };
        drawingSettingsError.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
        drawingSettingsError.SetShaderPassName(2, new ShaderTagId("Always"));
        drawingSettingsError.SetShaderPassName(3, new ShaderTagId("Vertex"));
        drawingSettingsError.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
        drawingSettingsError.SetShaderPassName(5, new ShaderTagId("VertexLM"));
        FilteringSettings filteringSettingsError = new FilteringSettings(RenderQueueRange.all);

        _context.DrawRenderers(_cullingResults, ref drawingSettingsError, ref filteringSettingsError);
    }

    // --------------------------------------------------------------------
    [Conditional("UNITY_EDITOR")]
    public static void PrepareCameraForSceneWindow(Camera _camera)
    {
        if (_camera.cameraType == CameraType.SceneView)
        {
            //ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }

    // --------------------------------------------------------------------
    public static bool SupportsComputeShaders()
    {
        return SystemInfo.supportsComputeShaders && !L2DLPipelineData.s_emulateNoComputeShaderSupport;
    }
}
