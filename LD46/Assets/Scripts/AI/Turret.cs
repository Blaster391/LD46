﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField]
    private GameObject m_currentTarget = null;
    [SerializeField]
    private float m_rateOfFire = 1.0f;
    [SerializeField]
    private float m_range = 1.0f;
    [SerializeField]
    private float m_damage = 1.0f;
    [SerializeField]
    private float m_projectileSpeed = 1.0f;
    [SerializeField]
    private float m_reactionTime = 1.0f;
    [SerializeField]
    private GameObject m_projectilePrefab;

    private bool m_isFiring = false;
    private float m_timeSinceUpdate = 0.0f;
    private float m_timeSinceLastShot = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_timeSinceUpdate = Random.Range(0, m_reactionTime);
    }

    // Update is called once per frame
    void Update()
    {
        m_timeSinceUpdate += Time.deltaTime;
        
        if (m_timeSinceUpdate > m_reactionTime)
        {
            UpdateFiringStatus();
        }

        if(m_isFiring)
        {
            m_timeSinceLastShot += Time.deltaTime;
            if(m_timeSinceLastShot > m_rateOfFire)
            {
                Fire();
            }
        }
    }

    void UpdateFiringStatus()
    {
        if(m_currentTarget != null && CanSeeTarget(m_currentTarget))
        {
            m_isFiring = true;
        }

        m_timeSinceUpdate = 0.0f;
    }

    bool CanSeeTarget(GameObject _target)
    {
        return true;
    }

    void Fire()
    {
        if (m_currentTarget != null)
        {
            GameObject spawnedProjectile = Instantiate<GameObject>(m_projectilePrefab);
            spawnedProjectile.transform.position = transform.position;

            TurretProjectile projectile = spawnedProjectile.GetComponent<TurretProjectile>();

            Vector2 directionToTarget = (m_currentTarget.transform.position - spawnedProjectile.transform.position).normalized;
            projectile.SetVelocity(directionToTarget * m_projectileSpeed);
        }
        m_timeSinceLastShot = 0.0f;
    }
}