using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEventPosition : MonoBehaviour
{
    [SerializeField]
    private float m_lifespan = 10.0f;

    private float m_timeAlive = 0.0f;

    // Update is called once per frame
    void Update()
    {
        m_timeAlive += Time.deltaTime;
        if(m_timeAlive > m_lifespan)
        {
            Destroy(gameObject);
        }
    }

    public void SetLifespan(float time)
    {
        m_lifespan = time;
    }
}
