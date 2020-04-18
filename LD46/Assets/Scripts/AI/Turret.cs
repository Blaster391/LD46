using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TurretSound;

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
    private GameObject m_projectilePrefab = null;

    public AK.Wwise.Event MyEvent;

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
        else
        {
            m_currentTarget = null;
            m_isFiring = false;

            var allEnemies = FindObjectsOfType<Enemy>();
            Vector2 myPosition = gameObject.transform.position;
            foreach(Enemy e in allEnemies)
            {
                Vector2 enemyPosition = e.transform.position;
                if ((enemyPosition - myPosition).magnitude > m_range)
                {
                    continue;
                }

                if(CanSeeTarget(e.gameObject))
                {
                    m_currentTarget = e.gameObject;
                    m_isFiring = true;
                    break;
                }
            }
        }


        m_timeSinceUpdate = 0.0f;
    }

    bool CanSeeTarget(GameObject _target)
    {
        return GameHelper.HasLineOfSight(gameObject, _target);
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
            MyEvent.Post(gameObject);
        }
        m_timeSinceLastShot = 0.0f;
    }
}
