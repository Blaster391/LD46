using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Transform))]
public class InteractionObject : MonoBehaviour
{
    private Collider2D m_collider2D = null;
    private Transform m_transform = null;

    private void OnDrawGizmos()
    {
        if(!m_transform || !m_collider2D) { return;  }

        var circle = m_collider2D as CircleCollider2D;
        if (circle)
        {
            Gizmos.DrawSphere(m_transform.position, m_collider2D.bounds.extents.x);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_collider2D = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        UnityEngine.Debug.Log(m_collider2D.friction.ToString());   
    }

    // Called when player drops this.
    public void OnDropped()
    {

    }

    // Called when player picks up
    public void OnPickup()
    {

    }
}
