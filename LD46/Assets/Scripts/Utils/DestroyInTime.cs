using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyInTime : MonoBehaviour
{
    [SerializeField] private float m_timeUntilDeath = 1f;
        
    void Update()
    {
        m_timeUntilDeath -= Time.deltaTime;
        if(m_timeUntilDeath < 0f)
        {
            Destroy(gameObject);
        }
    }
}
