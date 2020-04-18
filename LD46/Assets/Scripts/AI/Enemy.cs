using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private GameObject m_defaultTarget = null;

    [SerializeField]
    private float m_moveForce = 1.0f;

    [SerializeField]
    private float m_movementUpdateRate = 1.0f;

    private List<Vector2> m_path = null;
    private Rigidbody2D m_rigidbody2D = null;
    private NavMesh m_mesh;
    private float m_timeSinceLastPathUpdate = 0.0f;
    private Vector2 m_targetPosition = new Vector2();

    // Start is called before the first frame update
    void Start()
    {
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

        Vector2 targetPosition = m_defaultTarget.transform.position;
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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        OrbBehaviour orb = collision.gameObject.GetComponent<OrbBehaviour>();
        if (orb != null)
        {
            orb.TakeEnergy(-10.0f);
            Destroy(gameObject);
        }
    }
}
