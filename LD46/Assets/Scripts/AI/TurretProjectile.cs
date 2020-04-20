using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretProjectile : MonoBehaviour
{

    [SerializeField]
    private float m_lifeSpan = 10.0f;

    [SerializeField]
    private float m_damage = 10.0f;

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

        var directionToTarget = _velocity;
        var angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetDamage(float damage)
    {
         m_damage = damage;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == Tags.StaticWorld || collision.tag == Tags.BlockProjectiles)
        {
            Destroy(gameObject);
        }

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.DealDamage(m_damage);
            Destroy(gameObject);
        }
    }
}
