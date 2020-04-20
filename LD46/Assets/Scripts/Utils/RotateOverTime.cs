using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public float m_rotationSpeed = 1f;
    
    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(m_rotationSpeed * Time.deltaTime, Vector3.forward);
    }
}
