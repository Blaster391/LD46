using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private float m_availabilityUpdate = 1.0f;
    [SerializeField]
    private float m_minDistanceFromOrb = 10.0f;
    [SerializeField]
    private float m_maxDistanceFromOrb = 100.0f;


    private float m_timeSinceAvailabilityUpdate = 0.0f;

    private static List<EnemySpawner> s_enemySpawners = new List<EnemySpawner>();
    public static List<EnemySpawner> EnemySpawners { get { return new List<EnemySpawner>(s_enemySpawners); } }

    private void Awake()
    {
        s_enemySpawners.Add(this);
    }

    private void Start()
    {
        UpdateAvailability();
        m_timeSinceAvailabilityUpdate = Random.Range(0, m_availabilityUpdate);
    }

    private void OnDestroy()
    {
        s_enemySpawners.Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        m_timeSinceAvailabilityUpdate += Time.deltaTime;
        if(m_timeSinceAvailabilityUpdate > m_availabilityUpdate)
        {
            UpdateAvailability();
        }
    }

    private void UpdateAvailability()
    {
        m_timeSinceAvailabilityUpdate = 0.0f;

        OrbBehaviour orb = GameObject.FindObjectOfType<OrbBehaviour>();

        Vector2 myPosition = transform.position;
        Vector2 orbPosition = orb.transform.position;

        float distanceFromOrb = (myPosition - orbPosition).magnitude;

        if(distanceFromOrb > m_maxDistanceFromOrb || distanceFromOrb < m_minDistanceFromOrb)
        {
            IsAvailable = false;
        }
        else
        {
            IsAvailable = true;
        }
    }

    public bool IsAvailable { get; private set; } = true;
}
