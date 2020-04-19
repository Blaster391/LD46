using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private float m_yeetSpeedMultiplier = 20.0f;

    [SerializeField]
    private float m_forcePushMultiplier = 75.0f;

    [SerializeField]
    private float m_forcePushRadius = 7.5f;

    [SerializeField]
    [Range(1.0f, 180.0f)]
    private float m_forcePushConeAngle = 45.0f;

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

    void ForcePush()
    {
        Vector3 myPos = transform.position;
        Vector3 mousePos = Input.mousePosition;
        Vector3 dir = Camera.main.ScreenToWorldPoint(mousePos) - myPos;

        float cosConeAngle = Mathf.Cos(Mathf.Deg2Rad * m_forcePushConeAngle);

        int ignoreLayers = 
            (1 << 8) // static
            | (1 << 9); // player

        Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(myPos.x, myPos.y), m_forcePushRadius, ~ignoreLayers);

        foreach(Collider2D collider in colliders)
        {
            if (!collider.attachedRigidbody) continue;

            Vector3 toObj = collider.transform.position - myPos;
            float dot = Vector3.Dot(toObj.normalized, dir.normalized);
            if (toObj.magnitude < m_forcePushRadius && dot < cosConeAngle)
            {
                // Occlusion check
                bool fail = false;
                RaycastHit2D[] hits = Physics2D.RaycastAll(myPos, toObj.normalized, toObj.magnitude, ~(1 << 9)); // not player.
                foreach(RaycastHit2D hit in hits)
                {
                    if (hit.rigidbody?.gameObject == collider.gameObject) continue;
                    fail = true;
                }
                if (fail) continue;

                // Push the thing.
                float angMul = Mathf.Lerp(0.45f, 1.0f, (dot / cosConeAngle));
                float mul = m_timeMouseWasDownFor * m_forcePushMultiplier * angMul;
                collider.attachedRigidbody.AddForce(toObj * mul);
            }
        }
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

        if (m_mouseWasDown)
        {
            if (mouseDown)
            {
                m_timeMouseWasDownFor += Time.deltaTime;
            }
            else
            {
                if(m_objectInHands)
                {
                    YeetTheThing();
                }
                else
                {
                    ForcePush();
                }
                m_mouseWasDown = false;
                m_timeMouseWasDownFor = 0.0f;
            }
        }

        m_mouseWasDown = mouseDown;

        if (interactionPressed)
        {
            HandleInteraction();
        }
        
        if(usePressed && m_objectInHands != null)
        {
            m_objectInHands.Use(gameObject);
        }
    }
}
