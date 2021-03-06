﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(InteractionObject))]
public class OrbBehaviour : MonoBehaviour
{
    // Editor fields
    [Header("References")]
    [SerializeField] private L2DLPointLight m_pointLight = null;

    [Header("General")]
    [SerializeField] private float m_maxEnergy = 100f;
    [SerializeField] private float m_startingEnergy = 50f;

    [Header("Energy")]
    [SerializeField] private float m_energyLostPerSecond = 0f;
    [SerializeField] private float m_energyDrainedPerSecond = 0f;
    [SerializeField] private float m_maxEnergyGainPerSecondAtFullEnergy = 0f;
    [SerializeField] private float m_shopEnergyBuffer = 10.0f;

    private Dictionary<GameObject, int> m_purchasedItems = new Dictionary<GameObject, int>();

    public float ShopEnergyBuffer => m_shopEnergyBuffer;

    [System.Serializable]
    public class HealthScalingEffects
    {
        public bool m_usePointLightSettingsAtFullHealth = false;
        public HealthScaleProperties m_propsAtFullHealth = new HealthScaleProperties();
        public HealthScaleProperties m_propsAtAtNoHealth = new HealthScaleProperties();

        public float GetRangeAtProp(float _prop)
        {
            return Mathf.Lerp(m_propsAtAtNoHealth.m_range, m_propsAtFullHealth.m_range, _prop);
        }
        public float GetIntensityAtProp(float _prop)
        {
            return Mathf.Lerp(m_propsAtAtNoHealth.m_intensity, m_propsAtFullHealth.m_intensity, _prop);
        }
    }
    [System.Serializable]
    public class HealthScaleProperties
    {
        public float m_range;
        public float m_intensity;
    }
    [Header("Visuals")]
    [SerializeField] private HealthScalingEffects m_healthScalingEffects;
    [SerializeField] private Color m_standardColourTint = Color.yellow;
    [SerializeField] private Color m_consumingEnergyColourTint = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color m_gainingMaxHealthColourTint = Color.cyan;
    [SerializeField] private Color m_lowHealthColourTint = Color.red;
    [SerializeField] private Color m_yeetColourTint = Color.red;
    [SerializeField] private float m_lowHealthEnergyProp = 0.1f;
    [SerializeField] private ParticleSystem m_maxHealthGainParticleSystem = null;
    [SerializeField] private ParticleSystem m_yeetParticleSystem = null;

    [Header("UI")]
    [SerializeField] private ShopUIBehaviour m_shopUIPrefab = null;
    [SerializeField] private OrbUIBehaviour m_orbUIPrefab = null;

    // Working fields
    private bool m_isDead = false;
    private ShopUIBehaviour m_shopUI = null;
    private OrbUIBehaviour m_orbUI = null;
    private bool m_drainedEnergyThisFrame = false;
    // Public interface
    public float CurrentEnergy { get; private set; }
    public float CurrentEnergyProp { get { return CurrentEnergy / m_maxEnergy; } }
    public float CurrentRange { get; private set; }
    public bool IsShopOpen { get { return m_shopUI != null; } }

    // Components
    private GameWorldObjectManager m_gameWorldObjectManager = null;
    private InteractionObject m_interactionComponent = null;
    private SpriteRenderer m_spriteRenderer = null;

    public bool TakeEnergy(float _energyAmount)
    {
        if (!m_interactionComponent.IsInDamagingThrowState)
        {
            UpdateEnergy(-_energyAmount);
            return true;
        }
        return false;
    }

    public void PurchaseItem(GameObject _itemPrefab, float _energyCost)
    {
        if(CurrentEnergy >= _energyCost)
        {
            TakeEnergy(_energyCost);
            SpawnShopItem(_itemPrefab);
        }
    }

    public int GetPurchasedAmount(GameObject prefab)
    {
        if(!m_purchasedItems.ContainsKey(prefab))
        {
            return 0;
        }

        return m_purchasedItems[prefab];
    }

    private void Awake()
    {
        m_gameWorldObjectManager = GetComponentInParent<GameWorldObjectManager>();
        m_interactionComponent = GetComponent<InteractionObject>();
        m_interactionComponent.Used += Use;
        m_interactionComponent.Dropped += Dropped;
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        GameHelper.GetManager<StatsManager>().StartAliveTimer();

        CurrentEnergy = m_startingEnergy;

        if (m_healthScalingEffects.m_usePointLightSettingsAtFullHealth)
        {
            m_healthScalingEffects.m_propsAtFullHealth = new HealthScaleProperties()
            {
                m_range = m_pointLight.Range,
                m_intensity = m_pointLight.Intensity
            };
        }

        // Spawn UI
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas)
        {
            m_orbUI = Instantiate(m_orbUIPrefab, canvas.transform);
            m_orbUI.Initialise(this);
        }

        // Stop particle systems in case they're playing in the editor
        m_maxHealthGainParticleSystem.Stop();
        m_yeetParticleSystem.Stop();
    }

    void Update()
    {
        m_spriteRenderer.color = m_standardColourTint;
        DrainEnergy();
        if (!m_drainedEnergyThisFrame)
        {
            LoseEnergy();
        }
        IncreaseMaxEnergy();

        if(m_interactionComponent.IsInDamagingThrowState)
        {
            if(!m_yeetParticleSystem.isPlaying)
            {
                m_yeetParticleSystem.Play();
            }
            m_spriteRenderer.color = m_yeetColourTint;
        }
        else if (m_yeetParticleSystem.isPlaying)
        {
            m_yeetParticleSystem.Stop();
        }

        UpdateHealthScalingEffects();
    }

    void LoseEnergy()
    {
        UpdateEnergy(-m_energyLostPerSecond * Time.deltaTime);
    }

    void DrainEnergy()
    {
        bool drainedEnergy = false;
        foreach(DrainableBehaviour drainable in DrainableBehaviour.Drainables)
        {
            if (Vector3.Distance(transform.position, drainable.transform.position) < CurrentRange)
            {
                float energyTaken = drainable.TakeEnergy(m_energyDrainedPerSecond * Time.deltaTime);
                GameHelper.GetManager<StatsManager>().AddLeachedEnergy(energyTaken);
                UpdateEnergy(energyTaken);
                drainedEnergy = true;
            }
        }
        if (drainedEnergy)
        {
            m_spriteRenderer.color = m_consumingEnergyColourTint;
            
        }
        m_drainedEnergyThisFrame = drainedEnergy;
    }

    void IncreaseMaxEnergy()
    {
        if(CurrentEnergyProp == 1f)
        {
            m_maxEnergy += m_maxEnergyGainPerSecondAtFullEnergy * Time.deltaTime;
            m_spriteRenderer.color = m_gainingMaxHealthColourTint;

            if(!m_maxHealthGainParticleSystem.isPlaying)
            {
                m_maxHealthGainParticleSystem.Play();
            }
        }
        else
        {
            if(m_maxHealthGainParticleSystem.isPlaying)
            {
                m_maxHealthGainParticleSystem.Stop();
            }
        }
    }

    void UpdateHealthScalingEffects()
    {
        float energyProp = CurrentEnergyProp;
        float currentRange = m_healthScalingEffects.GetRangeAtProp(energyProp);
        CurrentRange = currentRange;

        m_pointLight.Range = currentRange;
        m_pointLight.Intensity = m_healthScalingEffects.GetIntensityAtProp(energyProp);

        if(energyProp < m_lowHealthEnergyProp && !m_drainedEnergyThisFrame)
        {
            m_spriteRenderer.color = m_lowHealthColourTint;
        }
    }

    private void UpdateEnergy(float _energyChange)
    {
        CurrentEnergy = Mathf.Clamp(CurrentEnergy + _energyChange, 0f, m_maxEnergy);
        if(CurrentEnergy == 0f && !m_isDead)
        {
            m_isDead = true;
            if(m_shopUI != null)
            {
                DestroyShopUI();
            }

            GameHelper.GetManager<GameSceneManager>().OnGameOver();
        }
    }

    private void SpawnShopItem(GameObject _turretPrefab)
    {
        if(m_purchasedItems.ContainsKey(_turretPrefab))
        {
            m_purchasedItems[_turretPrefab]++;
        }
        else
        {
            m_purchasedItems.Add(_turretPrefab, 1);
        }

        Instantiate(_turretPrefab, transform.position, Quaternion.identity, m_gameWorldObjectManager.TurretsParent);
    }

    private void Use(PlayerInteraction _playerInteraction)
    {
        if(m_shopUI == null)
        {
            SpawnShopUI();
        }
        else
        {
            DestroyShopUI();
        }
    }

    private void Dropped(PlayerInteraction _playerInteraction)
    {
        if (m_shopUI != null)
        {
            DestroyShopUI();
        }
    }

    // UI
    private void SpawnShopUI()
    {
        if(m_isDead)
        {
            return;
        }

        m_shopUI = Instantiate(m_shopUIPrefab, m_gameWorldObjectManager.UIParent);
        m_shopUI.Initialise(this);
    }

    private void DestroyShopUI()
    {
        Destroy(m_shopUI.gameObject);
        m_shopUI = null;
    }

    // Debug
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_healthScalingEffects.GetRangeAtProp(CurrentEnergyProp));
        Gizmos.color = Color.white;
    }
}
