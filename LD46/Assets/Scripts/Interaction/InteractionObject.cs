using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D), typeof(Joint2D))]
public class InteractionObject : MonoBehaviour
{
    private Joint2D m_joint2D = null;
    private Collider2D m_collider2D = null;
    private Rigidbody2D m_rigidBody2D = null;

    [SerializeField]
    private float m_pickupRadius = 0.5f;

    [SerializeField]
    private float m_sleepVelocityThreshold = 0.05f;

    [SerializeField]
    private float m_maxYeetVelocity = 20.0f;

    private bool m_isPickedUp = false;
    private bool m_isSettling = false;

    public float PickupRadius { get { return m_pickupRadius; } }

    public System.Action<PlayerInteraction> OnUse = delegate { };
    public System.Action<PlayerInteraction> Dropped = delegate { };

    private void OnSettle()
    {
        m_rigidBody2D.Sleep();
        m_rigidBody2D.simulated = false;
        m_collider2D.isTrigger = true;
        m_isSettling = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_rigidBody2D = GetComponent<Rigidbody2D>();
        m_joint2D = GetComponent<Joint2D>();
        m_collider2D = GetComponent<Collider2D>();

        OnSettle();
    }

    // Update is called once per frame
    void Update()
    {
        //if(m_isSettling)
        //{
        //    if(m_rigidBody2D.velocity.magnitude <= m_sleepVelocityThreshold)
        //    {
        //        OnSettle();
        //    }
        //}
    }

    // Called when player drops this.
    virtual public void OnDropped(GameObject player)
    {
        m_joint2D.connectedBody = null;
        m_isPickedUp = false;
        m_isSettling = true;
        m_joint2D.enabled = false;

        Dropped(player.GetComponent<PlayerInteraction>());
    }

    virtual public void OnYeeted(GameObject player, Vector3 force)
    {
        OnDropped(player);

        if(force.magnitude > m_maxYeetVelocity)
        {
            force = force.normalized * m_maxYeetVelocity;   
        }
        m_rigidBody2D.AddForce(force, ForceMode2D.Impulse);
    }

    // Called when player picks up
    virtual public void OnPickup(GameObject player)
    {

        m_joint2D.enabled = true;
        m_joint2D.connectedBody = player.GetComponent<Rigidbody2D>();
        m_rigidBody2D.WakeUp();
        m_rigidBody2D.simulated = true;
        m_collider2D.isTrigger = false;
        m_isSettling = false;
        m_isPickedUp = true;

        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), m_collider2D, true);
    }

    virtual public void Use(GameObject player)
    {
        OnUse(player.GetComponent<PlayerInteraction>());
    }
}
