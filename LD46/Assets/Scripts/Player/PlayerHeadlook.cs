using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadlook : MonoBehaviour
{
    [SerializeField]
    private float m_maxAngle = 75.0f;
    [SerializeField]
    private float m_minAngle = -75.0f;

    [SerializeField]
    private float m_leftOffset = 0.4f;

    [SerializeField]
    private SpriteRenderer m_bodySprite = null;
    private SpriteRenderer m_headSprite = null;




    // Start is called before the first frame update
    void Start()
    {
        m_headSprite = GetComponentInChildren<SpriteRenderer>();
    }

    bool m_headOffsetApplied = false;
    // Update is called once per frame
    void Update()
    {
        Vector2 myPosition = transform.position;

        if (m_headSprite.flipY && !m_headOffsetApplied)
        {
            m_headOffsetApplied = true;
            transform.position = transform.position + new Vector3(m_leftOffset, 0, 0);
        }
        else if(!m_headSprite.flipY && m_headOffsetApplied)
        {
            m_headOffsetApplied = false;
            transform.position = transform.position - new Vector3(m_leftOffset, 0, 0);
        }

        if (m_headOffsetApplied)
        {
            myPosition -= new Vector2(m_leftOffset, 0);
        }

        var directionToTarget = GameHelper.MouseToWorldPosition() - myPosition;
        var angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        if (Mathf.Abs(angle) > 90)
        {

             m_headSprite.flipY = true;

            if((angle > 0) && angle < (180 - m_maxAngle))
            {
                angle = (180 - m_maxAngle);
            }

            // I want to die
            if ((angle < 0) && (angle > -180 - m_minAngle))
            {
                angle = (-180 - m_minAngle);
            }
        }
        else                  
        {   

           m_headSprite.flipY = false;
            
            angle = Mathf.Max(angle, m_minAngle);
            angle = Mathf.Min(angle, m_maxAngle);
        }

        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);


    }
}
