using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyVisuals : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_bodySpriteRenderer;
    [SerializeField] private L2DL.L2DLSpriteBehaviour m_bodyL2DLSprite;

    [SerializeField] private L2DL.L2DLSpriteBehaviour m_stopsL2DLSprite;

    [SerializeField] private GameObject m_deathParticleSystem;

    private Color m_fullHealthColor;
    private float m_fullHealthEmission;

    [SerializeField] private Color m_deadColour;
    [SerializeField] private Color m_hitFlashColor;
    [SerializeField] private float m_flashTime = 0.25f;
    [SerializeField] private float m_flashEmissionMult = 2f;
    [SerializeField] private AnimationCurve m_flashEmissionCurve = new AnimationCurve();

    private bool m_flashingUwU = false;
    private float m_currentFlashTime = 0f;

    private Enemy m_enemy;

    private GameWorldObjectManager m_worldObjectManager;

    private void Awake()
    {
        m_enemy = GetComponent<Enemy>();

    }

    void Start()
    {
        m_worldObjectManager = GetComponentInParent<GameWorldObjectManager>();

        m_fullHealthColor = m_bodySpriteRenderer.color;
        m_fullHealthEmission = m_bodyL2DLSprite.Emission;

        m_enemy.OnHit += OnHit;
        m_enemy.OnDeath += OnDeath;
    }

    private void OnDestroy()
    {
        m_enemy.OnHit -= OnHit;
        m_enemy.OnDeath -= OnDeath;
    }

    void Update()
    {
        Color bodyColor = Color.Lerp(m_deadColour, m_fullHealthColor, m_enemy.CurrentEnergyProp);
        float bodyEmission = Mathf.Lerp(0f, m_fullHealthEmission, m_enemy.CurrentEnergyProp);

        if(m_flashingUwU)
        {
            m_currentFlashTime += Time.deltaTime;
            float flashProp = m_currentFlashTime / m_flashTime;

            float flashAmount = m_flashEmissionCurve.Evaluate(flashProp);
            Color actualColor = Color.Lerp(bodyColor, m_hitFlashColor, flashAmount);
            float emissionValue = Mathf.Lerp(bodyEmission, m_fullHealthEmission * m_flashEmissionMult, flashAmount);

            m_bodySpriteRenderer.color = actualColor;
            m_bodyL2DLSprite.Emission = emissionValue;

            if(m_currentFlashTime >= m_flashTime)
            {
                m_currentFlashTime = 0f;
                m_flashingUwU = false;
            }
        }
        else
        {
            m_bodySpriteRenderer.color = bodyColor;
            m_bodyL2DLSprite.Emission = bodyEmission;
        }
    }

    private void OnHit()
    {
        m_flashingUwU = true;
    }

    private void OnDeath()
    {
        if (m_worldObjectManager != null && m_worldObjectManager.EffectsParent != null)
        {
            Instantiate(m_deathParticleSystem, transform.position, Quaternion.identity, m_worldObjectManager.EffectsParent);
        }
    }
}
