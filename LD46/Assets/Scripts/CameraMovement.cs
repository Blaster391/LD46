using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform m_focus;
    [SerializeField] private float m_propFromFocusToMouse;
    [SerializeField] [Range(0, 1)] private float m_followSpeed;
    [SerializeField] private float m_maxDistFromFocus = 4f;

    private float m_z;

    Camera m_camera;

    private void Awake()
    {
        m_camera = GetComponent<Camera>();
        m_z = transform.position.z;
    }

    void Update()
    {
        Vector2 distanceFromFocus = (m_camera.ScreenToWorldPoint(Input.mousePosition) - m_focus.transform.position) * m_propFromFocusToMouse;
        if (distanceFromFocus.magnitude > m_maxDistFromFocus)
        {
            distanceFromFocus = distanceFromFocus.normalized * m_maxDistFromFocus;
        }
        Vector2 positionToGoTo = (Vector2)m_focus.transform.position + distanceFromFocus;
        Vector2 toMove = positionToGoTo - (Vector2)transform.position;
        Vector2 newPosition = (Vector2)transform.position + toMove * m_followSpeed;
        transform.position = new Vector3(newPosition.x, newPosition.y, m_z);
    }
}
