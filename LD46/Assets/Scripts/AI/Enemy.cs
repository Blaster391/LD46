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

        Vector2 targetPosition = m_defaultTarget.transform.position;
        Vector2 myPosition = transform.position;
        Vector2 directionToTarget = new Vector2();
        if (m_path != null && m_path.Count > 2)
        {
            directionToTarget = (m_path[2] - myPosition).normalized;
        }
        else
        {
            directionToTarget = (targetPosition - myPosition).normalized;
        }

        m_rigidbody2D.AddForce(directionToTarget * m_moveForce);
    }

    private void UpdatePath()
    {
        m_timeSinceLastPathUpdate = 0.0f;

        Vector2 targetPosition = m_defaultTarget.transform.position;
        Vector2 myPosition = transform.position;
        m_path = m_mesh.RequestPath(myPosition, targetPosition);
    }


    private void OnDrawGizmosSelected()
    {
        NavMesh mesh = GameHelper.GetManager<NavMesh>();
        mesh.DebugDrawPath(m_path);
    }

}
