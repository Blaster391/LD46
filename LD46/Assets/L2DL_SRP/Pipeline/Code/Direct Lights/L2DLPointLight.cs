using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * Stores the properties of a point light, ready to be processed by the L2DL SRP
 */
[RequireComponent(typeof(Camera))]
public class L2DLPointLight : MonoBehaviour, IL2DLDirectLight
{
    public float Range { get { return m_range; } set { m_range = value; } }

    // IL2DLDirectLight
    public Color Color { get { return m_color; } set { m_color = value; } }
    public float Intensity { get { return m_intensity; } set { m_intensity = value; } }
    public Camera ShadowCamera { get; private set; }
    public L2DLTextureSize ShadowMapSize { get { return m_shadowmapSize; } }
    public float MaxIntensityOutput { get { return m_maxIntensityOutput; } }

    [Header("Settings")]
    [SerializeField] private float m_intensity = 1f;
    [SerializeField] private Color m_color = Color.white;
    [SerializeField] private float m_range = 5f;

    [SerializeField] private L2DLTextureSize m_shadowmapSize = L2DLTextureSize.Size512;

    [SerializeField] private float m_maxIntensityOutput = 1f;

    // --------------------------------------------------------------------
    private void Start()
    {
        UpdateShadowCamera();
    }

    // --------------------------------------------------------------------
    private void OnValidate()
    {
        UpdateShadowCamera();
    }

    // --------------------------------------------------------------------
    private void UpdateShadowCamera()
    {
        if (ShadowCamera == null)
        {
            ShadowCamera = GetComponent<Camera>();
        }
        
        ShadowCamera.orthographicSize = Range;

        // Force some settings always
        ShadowCamera.orthographic = true;
        ShadowCamera.clearFlags = CameraClearFlags.SolidColor;
        ShadowCamera.backgroundColor = Color.clear;
        ShadowCamera.aspect = 1f;
    }

    // --------------------------------------------------------------------
    public bool IsWithinBounds(Bounds bounds)
    {
        Bounds pointLightBounds = new Bounds(transform.position, new Vector3(Range * 2, Range * 2, 0));
        if (bounds.Intersects(pointLightBounds))
        {
            return true;
        }
        return false;
    }

    // --------------------------------------------------------------------
    public bool IsSourceWithinCamera(Camera camera)
    {
        Vector3 screenPos = camera.WorldToScreenPoint(transform.position);
        if(screenPos.x >= 0 && screenPos.x < camera.pixelWidth && 
            screenPos.y >= 0 && screenPos.y < camera.pixelHeight)
        {
            return true;
        }
        return false;
    }

    // --------------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = m_color;
        Gizmos.DrawWireSphere(transform.position, Range);
    }

    // --------------------------------------------------------------------
#if UNITY_EDITOR
    [MenuItem("GameObject/L2DL/Direct Light/Point Light", false, 10)]
    static void CreateL2DLSpotLight(MenuCommand menuCommand)
    {
        var newL2DLPointLight = new GameObject("L2DLPointLight");
        newL2DLPointLight.tag = "L2DLDirectLight";
        GameObjectUtility.SetParentAndAlign(newL2DLPointLight, menuCommand.context as GameObject);
        Vector3 position = newL2DLPointLight.transform.position;
        position.z = -10;
        newL2DLPointLight.transform.position = position;
        Undo.RegisterCreatedObjectUndo(newL2DLPointLight, "Create " + newL2DLPointLight.name);
        Selection.activeObject = newL2DLPointLight;
        var spriteRenderer = newL2DLPointLight.AddComponent<L2DLPointLight>();
    }
#endif
}
