using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Vector2 m_maxVelocity = new Vector2(10.0f, 17.5f);
    [SerializeField]
    private float m_baseMovementForce = 1.0f;
    [SerializeField]
    private float m_baseJumpForce = 1.0f;

    [Range(0.0f, 1.0f)]
    [SerializeField]
    private float m_airDampening = 0.45f;

    [SerializeField]
    private float m_groundedEpsilon = 0.05f;


    private Rigidbody2D m_rigidbody2D = null;
    private CapsuleCollider2D m_capsuleCollider2D = null;

    private RaycastHit2D m_lastGroundHit;
    private bool m_isGrounded = true;

    private bool m_canDoubleJump = true;

    // Start is called before the first frame update
    void Start()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_capsuleCollider2D = GetComponent<CapsuleCollider2D>();
    }

    private void OnDrawGizmos()
    {
        if(m_lastGroundHit)
        {
            Gizmos.color = m_isGrounded ? Color.green : Color.red;
            Gizmos.DrawSphere(new Vector3(m_lastGroundHit.point.x, m_lastGroundHit.point.y, 0.0f), 0.1f);
        }
    }

    private void DoGroundCast()
    {
        var capsuleBounds = m_capsuleCollider2D.bounds;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(m_rigidbody2D.position, capsuleBounds.extents.x, Vector2.down);
        foreach (var hit in hits)
        {
            if (hit.rigidbody == m_rigidbody2D || hit.rigidbody?.gameObject?.GetComponent<InteractionObject>() != null) continue;

            m_lastGroundHit = hit;
            m_isGrounded = (m_rigidbody2D.position.y - hit.point.y) <= (capsuleBounds.extents.y + m_groundedEpsilon);
            break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool wasGrounded = m_isGrounded;
        DoGroundCast();

        if(!wasGrounded && m_isGrounded)
        {
            m_canDoubleJump = true;
        }

        bool leftMoveDown = Input.GetKey(KeyCode.A);
        bool rightMoveDown = Input.GetKey(KeyCode.D);

        float damp = m_isGrounded ? 1.0f : m_airDampening;

        if (leftMoveDown)
        {
            m_rigidbody2D.AddForce(-Vector2.right * m_baseMovementForce * damp);
        }

        if (rightMoveDown)
        {
            m_rigidbody2D.AddForce(Vector2.right * m_baseMovementForce * damp);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool doJump = m_isGrounded;
            if(!m_isGrounded && m_canDoubleJump)
            {
                m_canDoubleJump = false;
                doJump = true;
            }

            if(doJump)
            {
                m_rigidbody2D.AddForce(Vector2.up * m_baseJumpForce, ForceMode2D.Impulse);
            }
        }

        m_rigidbody2D.velocity = Vector2.Min(m_rigidbody2D.velocity, m_maxVelocity);
    }
}
