using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxVelocityClamp : MonoBehaviour
{
    [SerializeField]
    private bool m_clampDownwards = false;
    [SerializeField]
    private Vector2 m_maxVelocity = new Vector2(10, 10);

    private Rigidbody2D m_rigidbody2D = null;

    // Start is called before the first frame update
    void Start()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        

        m_rigidbody2D.velocity = Vector2.Min(m_rigidbody2D.velocity, m_maxVelocity);

        if(m_clampDownwards)
        {
            m_rigidbody2D.velocity = Vector2.Max(m_rigidbody2D.velocity, -m_maxVelocity);
        }
        else
        {
            // Don't clamp falling
            Vector2 velocity = m_rigidbody2D.velocity;
            velocity.x = Mathf.Max(velocity.x, -m_maxVelocity.x);
            m_rigidbody2D.velocity = velocity;
        }
    }
}
