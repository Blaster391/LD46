using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class L2DLPipelineData
{
    // General settings
    public static float s_backgroundDepthFromCamera;

    // Sorting settings
    public static SortingSettings SortingSettingsOpaque { get; } = new SortingSettings { criteria = SortingCriteria.CommonOpaque };
    public static SortingSettings SortingSettingsTransparent { get; } = new SortingSettings { criteria = SortingCriteria.CommonTransparent };

    // Drawing Settings
    public static DrawingSettings m_drawingSettingsOpaque;
    public static DrawingSettings m_drawingSettingsTransparent;

    // Filter Settings
    public static FilteringSettings m_filteringSettingsOpaque = new FilteringSettings(RenderQueueRange.opaque);
    public static FilteringSettings m_filteringSettingsTransparent = new FilteringSettings(RenderQueueRange.transparent);

    // Global Shader Properties
    public static int s_cameraColorTextureId = Shader.PropertyToID("_L2DLColorTexture");
    public static int s_cameraEmissionTextureId = Shader.PropertyToID("_L2DLEmissionTexture");
    public static int s_cameraOcclusionTextureId = Shader.PropertyToID("_L2DLOcclusionTexture");
    public static int s_cameraAdditionalDataTextureId = Shader.PropertyToID("_L2DLAdditionalDataTexture");
    public static int s_cameraDepthTextureId = Shader.PropertyToID("_L2DLDepthTexture");
    public static int s_cameraFakeDepthTextureId = Shader.PropertyToID("_L2DLFakeDepthTexture");

    public static int s_cameraDirectLightResultTextureId = Shader.PropertyToID("_L2DLDirectLightResultTexture");
    public static int s_cameraIndirectLightResultTextureId = Shader.PropertyToID("_L2DLIndirectLightResultTexture");

    // Materials - maybe move to an L2DL Resources later
    private static Material s_blitMaterial;
    public static Material BlitMaterial
    {
        get
        {
            if(s_blitMaterial == null)
            {
                s_blitMaterial = new Material(Shader.Find("My Pipeline/Blit"));
            }
            return s_blitMaterial;
        }
    }

    private static Material s_combineMaterial;
    public static Material CombineMaterial
    {
        get
        {
            if(s_combineMaterial == null)
            {
                s_combineMaterial = new Material(Shader.Find("L2DL/ColorLightCombinePass"));
            }
            return s_combineMaterial;
        }
    }

    // Debug
    public static bool s_emulateNoComputeShaderSupport = false;
}
