using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadlook : MonoBehaviour
{
    [SerializeField]
    private float m_maxAngle = 75.0f;
    [SerializeField]
    private float m_minAngle = -75.0f;

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

        Debug.Log(angle);

        if (Mathf.Abs(angle) > 90)
        {
            m_headSprite.flipY = true;
            m_bodySprite.flipX = true;

            if(angle > 0 && angle < 90 + m_maxAngle)
            {
                angle = 90 + m_maxAngle;
            }

            angle = -Mathf.Min(angle, -m_minAngle);
            angle = -Mathf.Max(angle, -m_maxAngle);

        }                     
        else                  
        {                     
            m_headSprite.flipY = false;
            m_bodySprite.flipX = false;

            angle = Mathf.Max(angle, m_minAngle);
            angle = Mathf.Min(angle, m_maxAngle);
        }

        //
        //

        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);


    }
}
