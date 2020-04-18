using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatComponent : MonoBehaviour
{
    [SerializeField]
    private float m_desiredHeight = 2.0f;
    [SerializeField]
    private float m_heightThreshold = 1.0f;
    [SerializeField]
    private float m_upwardsForce = 1.0f;

    private Rigidbody2D m_rigidbody2D = null;

    // Start is called before the first frame update
    void Start()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        int layerMask = 1 << 8;
        Vector2 myPosition2D = transform.position;

        float maxDistance = 100.0f;
        float distanceFromFloor = maxDistance;
        RaycastHit2D result = Physics2D.Raycast(myPosition2D, Vector2.down, maxDistance, layerMask);
        if (result)
        {
            distanceFromFloor = (myPosition2D - result.point).magnitude;
        }

        if (distanceFromFloor < m_desiredHeight)
        {
            m_rigidbody2D.gravityScale = 0.0f;
            m_rigidbody2D.AddForce(Vector2.up * m_upwardsForce);
        }
        if (distanceFromFloor + m_heightThreshold < m_desiredHeight)
        {
            float threshold = distanceFromFloor / m_heightThreshold;
            m_rigidbody2D.gravityScale = threshold;
        }
        else
        {

            m_rigidbody2D.gravityScale = 1.0f;
        }
    }
}
