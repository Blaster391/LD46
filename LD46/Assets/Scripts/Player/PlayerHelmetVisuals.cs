using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInteraction))]
public class PlayerHelmetVisuals : MonoBehaviour
{
    [SerializeField] private L2DL.L2DLSpriteBehaviour m_l2DLSprite;
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    [SerializeField] private L2DLSpotLight m_spotLight;
    private float m_startingEmission = 1f;
    [SerializeField] private float m_emissionMultiplierAtFullCharge = 2f;
    private Color m_visorStartingColor = Color.white;
    private Color m_spotLightStartingColor = Color.white;
    [SerializeField] private Color m_chargedColor = Color.red;

    [SerializeField] private ParticleSystem m_pushParticleSystem;
    [SerializeField] private ParticleSystem m_pullParticleSystem;
    [SerializeField] private int m_particlesAtFullCharge = 50;
    [SerializeField] private int m_minParticles = 20;

    PlayerInteraction m_playerInteraction;

    private void Awake()
    {
        m_playerInteraction = GetComponent<PlayerInteraction>();
        m_playerInteraction.OnForceEnergyPropChange += OnForceHoldChange;
        m_playerInteraction.OnPush += Push;
        m_playerInteraction.OnPull += Pull;
    }

    private void Start()
    {
        m_startingEmission = m_l2DLSprite.Emission;
        m_visorStartingColor = m_spriteRenderer.color;
        m_spotLightStartingColor = m_spotLight.Color;
    }

    void OnForceHoldChange(float _forceHoldProp)
    {
        m_l2DLSprite.Emission = m_startingEmission * (1 + (_forceHoldProp * (m_emissionMultiplierAtFullCharge - 1)));
        m_spriteRenderer.color = Color.Lerp(m_visorStartingColor, m_chargedColor, _forceHoldProp);
        m_spotLight.Color = Color.Lerp(m_spotLightStartingColor, m_chargedColor, _forceHoldProp);
    }

    void Push(float _forceProp)
    {
        int particlesToSpawn = (int)(m_particlesAtFullCharge * _forceProp);
        if (particlesToSpawn >= m_minParticles)
        {
            m_pushParticleSystem.Emit(particlesToSpawn);
        }
    }

    void Pull(float _forceProp)
    {
        int particlesToSpawn = (int)(m_particlesAtFullCharge * _forceProp);
        if (particlesToSpawn >= m_minParticles)
        {
            m_pullParticleSystem.Emit(particlesToSpawn);
        }
    }
}
