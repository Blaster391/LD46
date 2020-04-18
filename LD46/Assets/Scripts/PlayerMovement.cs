using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float m_maxVelocity = 10.0f;
    [SerializeField]
    private float m_baseMovementForce = 1.0f;
    [SerializeField]
    private float m_baseJumpForce = 1.0f;


    private Rigidbody2D m_rigidbody2D = null;

    // Start is called before the first frame update
    void Start()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.A))
        {
            m_rigidbody2D.AddForce(-Vector2.right * m_baseMovementForce);
        }
        if (Input.GetKey(KeyCode.D))
        {
            m_rigidbody2D.AddForce(Vector2.right * m_baseMovementForce);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_rigidbody2D.AddForce(Vector2.up * m_baseJumpForce, ForceMode2D.Impulse);
        }


        float speed = m_rigidbody2D.velocity.magnitude;
        if (speed > m_maxVelocity)
        {
            Vector3 movementDirection = m_rigidbody2D.velocity.normalized;
            m_rigidbody2D.velocity = movementDirection * m_maxVelocity;
        }
    }
}
