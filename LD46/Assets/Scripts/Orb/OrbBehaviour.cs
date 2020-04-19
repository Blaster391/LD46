using System.Collections;
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

    [Header("Energy")]
    [SerializeField] private float m_energyLostPerSecond = 0f;
    [SerializeField] private float m_energyDrainedPerSecond = 0f;
    [SerializeField] private float m_maxEnergyGainPerSecondAtFullEnergy = 0f;

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

    [Header("UI")]
    [SerializeField] private ShopUIBehaviour m_shopUIPrefab = null;
    [SerializeField] private OrbUIBehaviour m_orbUIPrefab = null;

    // Working fields
    private bool m_isDead = false;
    private ShopUIBehaviour m_shopUI = null;
    private OrbUIBehaviour m_orbUI = null;
    // Public interface
    public float CurrentEnergy { get; private set; }
    public float CurrentEnergyProp { get { return CurrentEnergy / m_maxEnergy; } }
    public float CurrentRange { get; private set; }

    // Components
    private GameWorldObjectManager m_gameWorldObjectManager = null;
    private InteractionObject m_interactionComponent = null;

    public void TakeEnergy(float _energyAmount)
    {
        UpdateEnergy(-_energyAmount);
    }

    public void PurchaseTurret(TurretBase _turretPrefab, float _energyCost)
    {
        if(CurrentEnergy >= _energyCost)
        {
            TakeEnergy(_energyCost);
            SpawnTurret(_turretPrefab);
        }
    }

    private void Awake()
    {
        m_gameWorldObjectManager = GetComponentInParent<GameWorldObjectManager>();
        m_interactionComponent = GetComponent<InteractionObject>();
        m_interactionComponent.Used += Use;
        m_interactionComponent.Dropped += Dropped;
    }

    void Start()
    {
        GameHelper.GetManager<StatsManager>().StartAliveTimer();

        CurrentEnergy = m_maxEnergy;

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
    }
    
    void Update()
    {
        LoseEnergy();
        DrainEnergy();
        IncreaseMaxEnergy();
        UpdateHealthScalingEffects();
    }

    void LoseEnergy()
    {
        UpdateEnergy(-m_energyLostPerSecond * Time.deltaTime);
    }

    void DrainEnergy()
    {
        foreach(DrainableBehaviour drainable in DrainableBehaviour.Drainables)
        {
            if (Vector3.Distance(transform.position, drainable.transform.position) < CurrentRange)
            {
                float energyTaken = drainable.TakeEnergy(m_energyDrainedPerSecond * Time.deltaTime);
                GameHelper.GetManager<StatsManager>().AddLeachedEnergy(energyTaken);
                UpdateEnergy(energyTaken);
            }
        }
    }

    void IncreaseMaxEnergy()
    {
        if(CurrentEnergyProp == 1f)
        {
            m_maxEnergy += m_maxEnergyGainPerSecondAtFullEnergy * Time.deltaTime;
        }
    }

    void UpdateHealthScalingEffects()
    {
        float energyProp = CurrentEnergyProp;
        float currentRange = m_healthScalingEffects.GetRangeAtProp(energyProp);
        CurrentRange = currentRange;

        m_pointLight.Range = currentRange;
        m_pointLight.Intensity = m_healthScalingEffects.GetIntensityAtProp(energyProp);
    }

    private void UpdateEnergy(float _energyChange)
    {
        CurrentEnergy = Mathf.Clamp(CurrentEnergy + _energyChange, 0f, m_maxEnergy);
        if(CurrentEnergy == 0f && !m_isDead)
        {
            m_isDead = true;
            GameHelper.GetManager<GameSceneManager>().OnGameOver();
        }
    }

    private void SpawnTurret(TurretBase _turretPrefab)
    {
        TurretBase turret = Instantiate(_turretPrefab, transform.position, Quaternion.identity, m_gameWorldObjectManager.TurretsParent);
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
