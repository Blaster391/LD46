using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DrainableBehaviour))]
public class DrainableVisualControlBehaviour : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_spriteRenderer;

    [Header("Colour")]
    [SerializeField] private bool m_useSpriteRenderableColour = false;
    [SerializeField] private Color m_fullHealthColour = Color.white;
    [SerializeField] private Color m_noHealthColour = Color.clear;

    [Header("Scale")]
    [SerializeField] private float m_fullhealthScale = 1f;
    [SerializeField] private float m_noHealthScale = 0.3f;

    Vector3 m_originalScale = Vector3.one;

    void Start()
    {
        GetComponent<DrainableBehaviour>().EnergyChange += UpdateVisuals;
        if(m_useSpriteRenderableColour)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if(spriteRenderer)
            {
                m_fullHealthColour = spriteRenderer.color;
            }
        }

        m_originalScale = transform.localScale;
    }

    void UpdateVisuals(DrainableBehaviour _drainable)
    {
        m_spriteRenderer.color = Color.Lerp(m_noHealthColour, m_fullHealthColour, _drainable.CurrentEnergyProp);
        float scale = Mathf.Lerp(m_noHealthScale, m_fullhealthScale, _drainable.CurrentEnergyProp);
        transform.localScale = m_originalScale * scale;
    }
}
