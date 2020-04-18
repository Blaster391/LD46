using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private float m_yeetSpeedMultiplier = 20.0f;

    private Collider2D m_collider = null;

    private InteractionObject m_objectInHands = null;

    private bool m_mouseWasDown = false;
    private float m_timeMouseWasDownFor = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_collider = GetComponent<Collider2D>();
    }

    private InteractionObject FindClosestObjectInRange()
    {
        Vector3 playerPos = transform.position;

        var objects = new List<InteractionObject>(GameObject.FindObjectsOfType<InteractionObject>());
        if (objects.Count > 0)
        {
            objects.Sort((a, b) => Vector3.SqrMagnitude(a.transform.position - playerPos).CompareTo(Vector3.SqrMagnitude(b.transform.position - playerPos)));
            foreach(var o in objects)
            {
                if(Vector3.SqrMagnitude(playerPos - o.transform.position) < (o.PickupRadius*o.PickupRadius))
                {
                    return o;
                }
            }
        }
        return null;
    }

    void DropTheThing()
    {
        m_objectInHands.OnDropped(gameObject);
        m_objectInHands = null;
    }

    void YeetTheThing()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 dir = Camera.main.ScreenToWorldPoint(mousePos) - transform.position;

        Vector3 v = dir.normalized * m_timeMouseWasDownFor * m_yeetSpeedMultiplier;
        m_objectInHands.OnYeeted(gameObject, v);
        m_objectInHands = null;
    }

    void HandleInteraction()
    {
        if (m_objectInHands)
        {
            DropTheThing();
        }
        else
        {
            var obj = FindClosestObjectInRange();
            if (obj)
            {
                m_objectInHands = obj;
                m_objectInHands.OnPickup(gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool interactionPressed = Input.GetKeyDown(KeyCode.F);
        bool usePressed = Input.GetKeyDown(KeyCode.E);
        bool mouseDown = !EventSystem.current.IsPointerOverGameObject() ? Input.GetMouseButton(0) : false;

        if(m_objectInHands)
        {
            if (m_mouseWasDown)
            {
                if (mouseDown)
                {
                    m_timeMouseWasDownFor += Time.deltaTime;
                }
                else
                {
                    YeetTheThing();
                    m_mouseWasDown = false;
                    m_timeMouseWasDownFor = 0.0f;
                }
            }
        }

        m_mouseWasDown = mouseDown;

        if(interactionPressed)
        {
            HandleInteraction();
        }
        
        if(usePressed && m_objectInHands != null)
        {
            m_objectInHands.Use(gameObject);
        }
    }
}
