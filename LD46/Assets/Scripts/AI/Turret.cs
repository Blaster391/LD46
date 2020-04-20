using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TurretSound;

public class Turret : MonoBehaviour
{
    [SerializeField]
    private GameObject m_firingPoint = null;

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

        if(m_currentTarget != null)
        {
            var directionToTarget = m_currentTarget.transform.position - transform.position;
            var angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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

        m_currentTarget = null;
        m_isFiring = false;

        var allEnemies = FindObjectsOfType<Enemy>();
        float rangeSq = m_range * m_range;

        allEnemies = allEnemies.Where(x => (transform.position - x.transform.position).sqrMagnitude < rangeSq).OrderBy(x => (transform.position - x.transform.position).sqrMagnitude).ToArray();

        foreach(Enemy e in allEnemies)
        {
            if(CanSeeTarget(e.gameObject))
            {
                m_currentTarget = e.gameObject;
                m_isFiring = true;
                break;
            }
        }

        m_timeSinceUpdate = 0.0f;
    }

    bool CanSeeTarget(GameObject _target)
    {
        return GameHelper.HasLineOfSight(m_firingPoint, _target);
    }

    void Fire()
    {
        if (m_currentTarget != null)
        {
            GameObject spawnedProjectile = Instantiate<GameObject>(m_projectilePrefab);

            Vector2 spawnPosition = (m_firingPoint != null) ? m_firingPoint.transform.position : transform.position;
            spawnedProjectile.transform.position = spawnPosition;
            TurretProjectile projectile = spawnedProjectile.GetComponent<TurretProjectile>();

            Vector2 directionToTarget = (m_currentTarget.transform.position - spawnedProjectile.transform.position).normalized;
            projectile.SetVelocity(directionToTarget * m_projectileSpeed);
            MyEvent.Post(gameObject);
        }
        m_timeSinceLastShot = 0.0f;
    }
}
