using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretProjectile : MonoBehaviour
{

    [SerializeField]
    private float m_lifeSpan = 10.0f;

    private float m_timeAlive = 0.0f;

    void Update()
    {
        m_timeAlive += Time.deltaTime;
        if(m_timeAlive > m_lifeSpan)
        {
            Destroy(gameObject);
        }
    }

    public void SetVelocity(Vector2 _velocity)
    {
        GetComponent<Rigidbody2D>().velocity = _velocity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == Tags.StaticWorld)
        {
            Destroy(gameObject);
        }

        if(collision.tag == Tags.Enemy)
        {
            // TODO damage
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
