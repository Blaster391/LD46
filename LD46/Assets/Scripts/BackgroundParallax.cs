using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    [SerializeField]
    private float m_maxScrollDistance = 100.0f;
    [SerializeField]
    private float m_maxScrollAmountForeground = 10.0f;
    [SerializeField]
    private float m_maxScrollAmountBackground = 1.0f;
    [SerializeField]
    private float m_backgroundZ = 10f;

    [SerializeField]
    private GameObject m_foreground;
    [SerializeField]
    private GameObject m_background;

    // Update is called once per frame
    void Update()
    {
        float scrollAmount = gameObject.transform.position.x / m_maxScrollDistance;
        scrollAmount = Mathf.Clamp(scrollAmount, -1, 1);
        scrollAmount = -scrollAmount;

        m_foreground.transform.localPosition = new Vector3(scrollAmount * m_maxScrollAmountForeground, 0, m_backgroundZ);
        m_background.transform.localPosition = new Vector3(scrollAmount * m_maxScrollAmountBackground, 0, m_backgroundZ);

    }
}
