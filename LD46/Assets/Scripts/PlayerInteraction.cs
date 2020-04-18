using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteraction : MonoBehaviour
{
    private Collider2D m_collider = null;

    private List<InteractionObject> m_overlappingInteractionObjects = new List<InteractionObject>();
    private InteractionObject m_objectInHands = null;

    // Start is called before the first frame update
    void Start()
    {
        m_collider = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        InteractionObject obj = other.gameObject.GetComponent<InteractionObject>();
        if (obj)
        {
            m_overlappingInteractionObjects.Add(obj);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        InteractionObject obj = other.gameObject.GetComponent<InteractionObject>();
        if (obj)
        {
            m_overlappingInteractionObjects.Remove(obj);
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool interactionPressed = Input.GetKeyDown(KeyCode.F);

        if(m_objectInHands)
        {
            if(interactionPressed)
            {
                m_objectInHands.OnDropped();
                m_objectInHands = null;
            }
            else
            {
                m_objectInHands.GetComponent<Transform>().position = transform.position;
            }

        }
        else if(m_overlappingInteractionObjects.Count > 0)
        {
            Vector3 playerPos = transform.position;
            m_overlappingInteractionObjects.Sort((a, b) => Vector3.SqrMagnitude(a.transform.position - playerPos).CompareTo(Vector3.SqrMagnitude(b.transform.position - playerPos)));
            if(interactionPressed)
            {
                m_objectInHands = m_overlappingInteractionObjects[0];
            }
        }
    }
}
