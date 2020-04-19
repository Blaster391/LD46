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

    private SpriteRenderer m_bodySprite = null;
    private SpriteRenderer m_headSprite = null;




    // Start is called before the first frame update
    void Start()
    {
        m_headSprite = GetComponentInChildren<SpriteRenderer>();
        m_bodySprite = gameObject.transform.parent.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 myPosition = transform.position;
        var directionToTarget = GameHelper.MouseToWorldPosition() - myPosition;
        var angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

        if (Mathf.Abs(angle) > 90)
        {
            if(!m_headSprite.flipY)
            {
                m_headSprite.flipY = true;
                m_bodySprite.flipX = true;

                transform.position = transform.position + new Vector3(m_leftOffset, 0, 0);
            }


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
            if(m_headSprite.flipY)
            {


                m_headSprite.flipY = false;
                m_bodySprite.flipX = false;

                transform.position = transform.position - new Vector3(m_leftOffset, 0, 0);
            }


            angle = Mathf.Max(angle, m_minAngle);
            angle = Mathf.Min(angle, m_maxAngle);
        }

        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);


    }
}
