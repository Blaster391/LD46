using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private GameObject m_target = null;

    [SerializeField]
    private float m_energy = 10.0f;
    private float m_maxEnergy = 10f;
    [SerializeField]
    private float m_energyAttackModifier = 1.0f;

    [SerializeField]
    private float m_moveForce = 1.0f;

    [SerializeField]
    private float m_movementUpdateRate = 1.0f;

    [SerializeField]
    private float m_despawnRange = 75.0f;
    [SerializeField]
    private float m_despawnUpdateRate = 1.0f;
    private float m_timeSinceLastDespawnCheck = 0.0f;

    private List<Vector2> m_path = null;
    private Rigidbody2D m_rigidbody2D = null;
    private NavMesh m_mesh;
    private float m_timeSinceLastPathUpdate = 0.0f;
    private Vector2 m_targetPosition = new Vector2();
    private float m_size = 0.1f;

    public float CurrentEnergyProp { get { return m_energy / m_maxEnergy; } }

    public AK.Wwise.Event MyEvent;
    public AK.Wwise.Event MyEvent2;

    // Static tracking
    private static List<Enemy> s_enemies = new List<Enemy>();
    public static List<Enemy> Enemies { get { return new List<Enemy>(s_enemies); } }

    public System.Action OnHit = delegate { };
    public System.Action OnDeath = delegate { };

    private void Awake()
    {
        s_enemies.Add(this);
    }

    void Start()
    {
        m_maxEnergy = m_energy;

        m_target = FindObjectOfType<OrbBehaviour>().gameObject;
        m_mesh = GameHelper.GetManager<NavMesh>();
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_timeSinceLastPathUpdate = Random.Range(0, m_movementUpdateRate);

        m_size = GetComponent<CircleCollider2D>().radius;

        Physics2D.IgnoreCollision(FindObjectOfType<PlayerMovement>().GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
    }

    void OnDestroy()
    {
        s_enemies.Remove(this);
    }
    
    void Update()
    {
        m_timeSinceLastDespawnCheck += Time.deltaTime;
        m_timeSinceLastPathUpdate += Time.deltaTime;
        if(m_timeSinceLastDespawnCheck > m_despawnUpdateRate)
        {
            CheckDespawn();
        }


        if (m_timeSinceLastPathUpdate > m_movementUpdateRate)
        {
            UpdatePath();
        }

        if(m_path != null)
        {
            UpdatePathProgress();
        }

        Vector2 myPosition = transform.position;
        Vector2 directionToTarget = (m_targetPosition - myPosition).normalized;

        m_rigidbody2D.AddForce(directionToTarget * m_moveForce);
    }

    void CheckDespawn()
    {
        m_timeSinceLastDespawnCheck = 0.0f;
        OrbBehaviour orb = FindObjectOfType<OrbBehaviour>();
        float despawnRangeSq = m_despawnRange * m_despawnRange;
        if ((transform.position - orb.transform.position).sqrMagnitude > despawnRangeSq)
        {
            PlayerMovement player = FindObjectOfType<PlayerMovement>();
            if ((transform.position - player.transform.position).sqrMagnitude > despawnRangeSq)
            {
                Destroy(gameObject);
            }
        }
    }

    void UpdatePathProgress()
    {
        if (m_path.Count == 0)
        {
            m_targetPosition = m_target.transform.position;
            return;
        }


        if (m_path.Count > 1 && (GameHelper.HasLineOfSight(gameObject, m_path[1])))
        {
            m_targetPosition = m_path[1];
            m_path.RemoveAt(0);

        }
        else
        {
            m_targetPosition = m_path[0];
        }
    }

    private void UpdatePath()
    {
        m_timeSinceLastPathUpdate = 0.0f;

        Vector2 targetPosition = m_target.transform.position;
        Vector2 myPosition = transform.position;
        var newPath = m_mesh.RequestPath(myPosition, targetPosition);
        m_targetPosition = targetPosition;
        if (newPath != null)
        {
            m_path = newPath;
            if(m_path.Count > 0)
            {
                m_path.RemoveAt(0);
            }

            UpdatePathProgress();
        }
    }

    private void OnDrawGizmosSelected()
    {
        NavMesh mesh = GameHelper.GetManager<NavMesh>();
        mesh.DebugDrawPath(m_path);

        Gizmos.DrawSphere(m_targetPosition, m_size);
    }

    public void DealDamage(float damage)
    {
        m_energy -= damage;
        OnHit();
        if (m_energy <= 0)
        {
            GameHelper.GetManager<StatsManager>().IncrementKills();
            OnDeath();
            GameHelper.GetManager<AudioEventManager>().MakeAudioEvent(transform.position, 5.0f, MyEvent);
            Destroy(gameObject);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject otherObject)
    {
        OrbBehaviour orb = otherObject.GetComponent<OrbBehaviour>();
        if (orb != null)
        {
            GameHelper.GetManager<AudioEventManager>().MakeAudioEvent(transform.position, 5.0f, MyEvent2);
            orb.TakeEnergy(m_energy * m_energyAttackModifier);
            Destroy(gameObject);
        }
    }
}
