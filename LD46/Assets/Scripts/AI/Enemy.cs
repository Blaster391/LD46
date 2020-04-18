using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private GameObject m_defaultTarget = null;

    [SerializeField]
    private float m_moveForce = 1.0f;

    private Rigidbody2D m_rigidbody2D = null;

    // Start is called before the first frame update
    void Start()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {


        Vector2 targetPosition = m_defaultTarget.transform.position;
        Vector2 myPosition = transform.position;
        Vector2 directionToTarget = new Vector2();

        NavMesh mesh = GameHelper.GetManager<NavMesh>();
        var path = mesh.RequestPath(myPosition, targetPosition);

        if(path != null && path.Count > 2)
        {
            directionToTarget = (path[2] - myPosition).normalized;
            _debugPath = path;
        }
        else
        {
            directionToTarget = (targetPosition - myPosition).normalized;
        }

        m_rigidbody2D.AddForce(directionToTarget * m_moveForce);
    }

    List<Vector2> _debugPath = null;
    private void OnDrawGizmosSelected()
    {
        NavMesh mesh = GameHelper.GetManager<NavMesh>();
        mesh.DebugDrawPath(_debugPath);
    }

}
