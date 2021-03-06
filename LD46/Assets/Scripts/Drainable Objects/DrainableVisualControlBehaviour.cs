﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DrainableBehaviour))]
public class DrainableVisualControlBehaviour : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_spriteRenderer = null;
    [SerializeField] private ParticleSystem m_drainingParticleSystem = null;

    [Header("Colour")]
    [SerializeField] private bool m_useSpriteRenderableColour = false;
    [SerializeField] private List<Color> m_fullHealthColours = new List<Color>();
    private Color m_fullHealthColour = Color.white;
    [SerializeField] private Color m_noHealthColour = Color.clear;

    [Header("Scale")]
    [SerializeField] private float m_fullhealthScale = 1f;
    [SerializeField] private float m_noHealthScale = 0.3f;

    Vector3 m_originalScale = Vector3.one;

    bool m_isBeingDrained = false;
    bool m_wasBeingDrained = false;

    void Start()
    {
        GetComponent<DrainableBehaviour>().EnergyChange += UpdateVisuals;
        if(m_useSpriteRenderableColour)
        {
            m_fullHealthColour = m_spriteRenderer.color;
        }
        else
        {
            m_fullHealthColour = m_fullHealthColours[Random.Range(0, m_fullHealthColours.Count)];
            m_spriteRenderer.color = m_fullHealthColour;
        }

        m_originalScale = transform.localScale;

        m_drainingParticleSystem.Stop();
    }

    private void Update()
    {
        if(m_wasBeingDrained)
        {
            if (!m_isBeingDrained)
            {
                m_drainingParticleSystem.Stop();
                m_wasBeingDrained = false;
            }
        }

        if(m_isBeingDrained)
        {
            m_wasBeingDrained = true;
        }

        m_isBeingDrained = false;
    }

    void UpdateVisuals(DrainableBehaviour _drainable)
    {
        m_spriteRenderer.color = Color.Lerp(m_noHealthColour, m_fullHealthColour, _drainable.CurrentEnergyProp);
        float scale = Mathf.Lerp(m_noHealthScale, m_fullhealthScale, _drainable.CurrentEnergyProp);
        transform.localScale = m_originalScale * scale;

        if(!m_drainingParticleSystem.isPlaying)
        {
            m_drainingParticleSystem.Play();
        }
        m_isBeingDrained = true;
    }
}
