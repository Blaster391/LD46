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

    [SerializeField]
    private AudioEventPosition m_deathObject = null;

    private List<Vector2> m_path = null;
    private Rigidbody2D m_rigidbody2D = null;
    private NavMesh m_mesh;
    private float m_timeSinceLastPathUpdate = 0.0f;
    private Vector2 m_targetPosition = new Vector2();

    // Start is called before the first frame update
    void Start()
    {
        m_target = GameObject.FindObjectOfType<OrbBehaviour>().gameObject;
        m_mesh = GameHelper.GetManager<NavMesh>();
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_timeSinceLastPathUpdate = Random.Range(0, m_movementUpdateRate);
    }

    // Update is called once per frame
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

            // PLAY AUDIO
            //GameHelper.GetManager<AudioEventManager>().MakeAudioEvent(transform.position, 5.0f, EVENT);

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

    void HandleCollision(GameObject collision)
    {
        OrbBehaviour orb = collision.GetComponent<OrbBehaviour>();
        if (orb != null)
        {
            orb.TakeEnergy(m_energy * m_energyAttackModifier);
            Destroy(gameObject);
        }
    }
}
