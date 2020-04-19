using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private GameObject m_target = null;

    [SerializeField]
    private float m_energy = 10.0f;
    [SerializeField]
    private float m_energyAttackModifier = 1.0f;

    [SerializeField]
    private float m_moveForce = 1.0f;

    [SerializeField]
    private float m_movementUpdateRate = 1.0f;

    private List<Vector2> m_path = null;
    private Rigidbody2D m_rigidbody2D = null;
    private NavMesh m_mesh;
    private float m_timeSinceLastPathUpdate = 0.0f;
    private Vector2 m_targetPosition = new Vector2();

    public AK.Wwise.Event MyEvent;

    // Static tracking
    private static List<Enemy> s_enemies = new List<Enemy>();
    public static List<Enemy> Enemies { get { return new List<Enemy>(s_enemies); } }

    private void Awake()
    {
        s_enemies.Add(this);
    }

    void Start()
    {
        m_target = FindObjectOfType<OrbBehaviour>().gameObject;
        m_mesh = GameHelper.GetManager<NavMesh>();
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_timeSinceLastPathUpdate = Random.Range(0, m_movementUpdateRate);
    }

    void OnDestroy()
    {
        s_enemies.Remove(this);
    }
    
    void Update()
    {

        m_timeSinceLastPathUpdate += Time.deltaTime;
        if (m_timeSinceLastPathUpdate > m_movementUpdateRate)
        {
            UpdatePath();
        }

        Vector2 myPosition = transform.position;
        Vector2 directionToTarget = (m_targetPosition - myPosition).normalized;

        m_rigidbody2D.AddForce(directionToTarget * m_moveForce);
    }

    private void UpdatePath()
    {
        m_timeSinceLastPathUpdate = 0.0f;

        Vector2 targetPosition = m_target.transform.position;
        Vector2 myPosition = transform.position;
        var newPath = m_mesh.RequestPath(myPosition, targetPosition);
        if(newPath != null)
        {
            m_path = newPath;
        }

        m_targetPosition = targetPosition;
        if (m_path != null && m_path.Count > 2)
        {
            //if (GameHelper.IsWithinThreshold(myPosition, m_path[2], 0.1f))
            //{
            //    m_path.RemoveAt(2);
            //}

            //if (GameHelper.IsWithinThreshold(myPosition, m_path[1], 0.1f))
            //{
            //    m_path.RemoveAt(1);
            //}

            if(GameHelper.IsWithinThreshold(myPosition, m_path[1], 0.1f) || GameHelper.HasLineOfSight(gameObject, m_path[2]))
            {
                m_targetPosition = m_path[2];

                if (GameHelper.IsWithinThreshold(myPosition, m_path[2], 0.1f))
                {
                    m_path.RemoveAt(2);
                }
            }
            else
            {
                m_targetPosition = m_path[1];
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        NavMesh mesh = GameHelper.GetManager<NavMesh>();
        mesh.DebugDrawPath(m_path);
    }

    public void DealDamage(float damage)
    {
        m_energy -= damage;
        if(m_energy <= 0)
        {
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
            orb.TakeEnergy(m_energy * m_energyAttackModifier);
            Destroy(gameObject);
        }
    }
}
