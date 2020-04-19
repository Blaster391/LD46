using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToOrb : MonoBehaviour
{
    OrbBehaviour m_orb;

    private void Awake()
    {
        m_orb = FindObjectOfType<OrbBehaviour>();
    }

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, m_orb.transform.position - transform.position);
    }
}
