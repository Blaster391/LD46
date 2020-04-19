using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * Stores the properties of a spot light, ready to be processed by the L2DL SRP
 */
[RequireComponent(typeof(Camera))]
public class L2DLSpotLight : MonoBehaviour, IL2DLDirectLight
{
    public float Range { get { return m_range; } set { m_range = value; } }
    public float Angle { get { return m_angle; } set { m_angle = value; } }
    private float Width { get { return 2 * Mathf.Tan(Angle / 2f * Mathf.Deg2Rad) * Range; } }
    public float MaxIntensityOutput { get { return m_maxIntensityOutput; } }
    public float StartingRange { get { return m_startingRange; } }

    // IL2DLDirectLight
    public Color Color { get { return m_color; } set { m_color = value; } }
    public float Intensity { get { return m_intensity; } set { m_intensity = value; } }
    public Camera ShadowCamera { get; private set; }
    public L2DLTextureSize ShadowMapSize { get { return m_shadowmapSize; } }

    [Header("Settings")]
    [SerializeField] private float m_intensity = 1f;
    [SerializeField] private Color m_color = Color.white;
    [SerializeField] private float m_range = 5f;
    [SerializeField] private float m_angle = 70f;

    [SerializeField] private L2DLTextureSize m_shadowmapSize = L2DLTextureSize.Size512;

    [SerializeField] private float m_maxIntensityOutput = 1f;
    [SerializeField] private float m_startingRange = 0.5f;

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
        
        ShadowCamera.orthographicSize = Range / 2f;
        ShadowCamera.aspect = Range != 0 ? Width / Range : 1f;

        // Force some settings always
        ShadowCamera.orthographic = true;
        ShadowCamera.clearFlags = CameraClearFlags.SolidColor;
        ShadowCamera.backgroundColor = Color.clear;
    }

    // --------------------------------------------------------------------
    public bool IsWithinBounds(Bounds bounds)
    {
        Bounds spotLightBounds = new Bounds(transform.position, Vector3.zero);
        Vector3 sourcePoint = transform.position + Range / 2f * transform.up;
        Vector3 basePoint = transform.position - Range / 2f * transform.up;
        Vector3 endPointOffset = Width / 2 * transform.right;

        spotLightBounds.Encapsulate(sourcePoint);
        spotLightBounds.Encapsulate(basePoint + endPointOffset);
        spotLightBounds.Encapsulate(basePoint - endPointOffset);

        if (bounds.Intersects(spotLightBounds))
        {
            return true;
        }
        return false;
    }

    // --------------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        Vector3 sourcePoint = transform.position + Range / 2f * transform.up;
        Vector3 centerPoint = transform.position - Range / 2f * transform.up;
        Vector3 endPointOffset = Width / 2 * transform.right;

        Gizmos.color = Color;
        Gizmos.DrawLine(sourcePoint, centerPoint + endPointOffset);
        Gizmos.DrawLine(sourcePoint, centerPoint - endPointOffset);
        Gizmos.DrawLine(centerPoint + endPointOffset, centerPoint - endPointOffset);
    }

    // --------------------------------------------------------------------
#if UNITY_EDITOR
    [MenuItem("GameObject/L2DL/Direct Light/Spot Light", false, 10)]
    static void CreateL2DLSpotLight(MenuCommand menuCommand)
    {
        var newL2DLPointLightHolder = new GameObject("L2DLSpotLightHolder");
        GameObjectUtility.SetParentAndAlign(newL2DLPointLightHolder, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(newL2DLPointLightHolder, "Create " + newL2DLPointLightHolder.name);

        var newL2DLPointLight = new GameObject("L2DLSpotLight");
        newL2DLPointLight.tag = "L2DLDirectLight";
        GameObjectUtility.SetParentAndAlign(newL2DLPointLight, newL2DLPointLightHolder);
        Vector3 position = newL2DLPointLight.transform.position;
        position.z = -10;
        newL2DLPointLight.transform.position = position;
        Undo.RegisterCreatedObjectUndo(newL2DLPointLight, "Create " + newL2DLPointLight.name);
        Selection.activeObject = newL2DLPointLight;
        newL2DLPointLight.AddComponent<L2DLSpotLight>();
        newL2DLPointLight.AddComponent<L2DLSpotLightPositioningAssistance>();
    }
#endif
}
