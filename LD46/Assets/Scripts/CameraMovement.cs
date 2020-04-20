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

    Camera m_camera;

    private void Awake()
    {
        m_camera = GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 distanceFromFocus = (m_camera.ScreenToWorldPoint(Input.mousePosition) - m_focus.transform.position) * m_propFromFocusToMouse;
        if (distanceFromFocus.magnitude > m_maxDistFromFocus)
        {
            distanceFromFocus = distanceFromFocus.normalized * m_maxDistFromFocus;
        }
        Vector3 positionToGoTo = m_focus.transform.position + distanceFromFocus;
        Vector3 toMove = positionToGoTo - transform.position;
        transform.position += toMove * m_followSpeed;
    }
}
