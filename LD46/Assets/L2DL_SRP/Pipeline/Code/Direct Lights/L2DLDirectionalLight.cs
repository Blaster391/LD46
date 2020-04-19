using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * Stores the properties of a directional light, ready to be processed by the L2DL SRP
 */
[RequireComponent(typeof(Camera))]
public class L2DLDirectionalLight : MonoBehaviour, IL2DLDirectLight
{    
    public float Width { get { return m_width; } set { m_width = value; } }
    public float Height { get { return m_height; } set { m_height = value; } }

    // IL2DLDirectLight
    public Color Color { get { return m_color; } set { m_color = value; } }
    public float Intensity { get { return m_intensity; } set { m_intensity = value; } }
    public Camera ShadowCamera { get; private set; }
    public L2DLTextureSize ShadowMapSize { get { return m_shadowmapSize; } }

    [Header("Settings")]
    [SerializeField] private float m_intensity = 1f;
    [SerializeField] private Color m_color = Color.white;

    [SerializeField] private float m_width = 20f;
    [SerializeField] private float m_height = 20f;

    [SerializeField] private L2DLTextureSize m_shadowmapSize = L2DLTextureSize.Size512;

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
        if(ShadowCamera == null)
        {
            ShadowCamera = GetComponent<Camera>();
        }

        ShadowCamera.orthographicSize = m_height / 2f;
        ShadowCamera.aspect = m_width / m_height;

        // Force some settings always
        ShadowCamera.orthographic = true;
        ShadowCamera.clearFlags = CameraClearFlags.SolidColor;
        ShadowCamera.backgroundColor = Color.clear;
    }

    // --------------------------------------------------------------------
    public bool IsWithinBounds(Bounds bounds)
    {
        Bounds lightBounds = new Bounds(transform.position, Vector3.zero);
        Matrix4x4 lightMatrix = transform.localToWorldMatrix;
        Vector2 halfSize = new Vector2(m_width / 2, m_height / 2);
        halfSize = lightMatrix.MultiplyVector(halfSize);
        lightBounds.Encapsulate(halfSize);
        lightBounds.Encapsulate(-halfSize);
        if (bounds.Intersects(lightBounds))
        {
            return true;
        }
        return false;
    }

    // --------------------------------------------------------------------
    public bool IsSourceWithinCamera(Camera camera)
    {
        Vector3 topLeft = new Vector2(-m_width / 2, m_height / 2);
        Vector3 topRight = new Vector2(m_width / 2, m_height / 2);

        Matrix4x4 lightMatrix = transform.localToWorldMatrix;

        topLeft = lightMatrix.MultiplyVector(topLeft);
        topRight = lightMatrix.MultiplyVector(topRight);

        Vector3 screenPosTopLeft = camera.WorldToScreenPoint(transform.position + topLeft);
        Vector3 screenPosTopRight = camera.WorldToScreenPoint(transform.position + topRight);
        if (screenPosTopLeft.x >= 0 && screenPosTopLeft.x < camera.pixelWidth
            && screenPosTopLeft.y >= 0 && screenPosTopLeft.y < camera.pixelHeight
            && screenPosTopRight.x >= 0 && screenPosTopRight.x < camera.pixelWidth
            && screenPosTopRight.y >= 0 && screenPosTopRight.y < camera.pixelHeight)
        {
            return true;
        }
        return false;
    }

    // --------------------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = m_color;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(Width, Height, 0));

        int arrowsToDraw = 3;
        float arrowTop = Height / 8f * 3f;
        float arrowBottom = -arrowTop;
        float arrowHalfWidth = Width / 12f;
        float arrowSeperation = Width / (arrowsToDraw + 1);

        float startX = -Width / 2 + arrowSeperation;
        for (int lightIndex = 0; lightIndex < 3; ++lightIndex)
        {
            float arrowX = startX + lightIndex * arrowSeperation;

            Gizmos.DrawLine(new Vector3(arrowX, arrowTop, 0), new Vector3(arrowX, arrowBottom, 0));

            Gizmos.DrawLine(new Vector3(arrowX - arrowHalfWidth, arrowBottom + arrowHalfWidth, 0), new Vector3(arrowX, arrowBottom, 0));
            Gizmos.DrawLine(new Vector3(arrowX + arrowHalfWidth, arrowBottom + arrowHalfWidth, 0), new Vector3(arrowX, arrowBottom, 0));
        }
    }

    // --------------------------------------------------------------------
#if UNITY_EDITOR
    [MenuItem("GameObject/L2DL/Direct Light/Directional Light", false, 10)]
    static void CreateL2DLSpotLight(MenuCommand menuCommand)
    {
        var newL2DLPointLight = new GameObject("L2DLDirectionalLight");
        newL2DLPointLight.tag = "L2DLDirectLight";
        GameObjectUtility.SetParentAndAlign(newL2DLPointLight, menuCommand.context as GameObject);
        Vector3 position = newL2DLPointLight.transform.position;
        position.z = -10;
        newL2DLPointLight.transform.position = position;
        Undo.RegisterCreatedObjectUndo(newL2DLPointLight, "Create " + newL2DLPointLight.name);
        Selection.activeObject = newL2DLPointLight;
        var spriteRenderer = newL2DLPointLight.AddComponent<L2DLDirectionalLight>();
    }
#endif
}
