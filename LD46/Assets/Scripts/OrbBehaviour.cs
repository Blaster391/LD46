using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OrbBehaviour : MonoBehaviour
{
    // Editor fields
    [Header("References")]
    [SerializeField] private L2DLPointLight m_pointLight = null;

    [Header("General")]
    [SerializeField] private float m_maxEnergy;

    [Header("Energy")]
    [SerializeField] private float m_energyLostPerSecond;
    [SerializeField] private float m_energyDrainedPerSecond;

    [System.Serializable]
    struct HealthScalingEffects
    {
        public bool m_usePointLightSettingsAtFullHealth;
        public HealthScaleProperties m_propsAtFullHealth;
        public HealthScaleProperties m_propsAtAtNoHealth;

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
    struct HealthScaleProperties
    {
        public float m_range;
        public float m_intensity;
    }
    [Header("Visuals")]
    [SerializeField] private HealthScalingEffects m_healthScalingEffects;

    // Working fields

    // Public interface
    public float CurrentEnergy { get; private set; }
    public float CurrentEnergyProp { get { return CurrentEnergy / m_maxEnergy; } }
    public float CurrentRange { get; private set; }

    public void TakeEnergy(float _energyAmount)
    {
        UpdateEnergy(-_energyAmount);
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
    }
    
    void Update()
    {
        LoseEnergy();
        DrainEnergy();
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
        if(CurrentEnergy == 0f)
        {
            //Dead
            GameHelper.GetManager<StatsManager>().EndAliveTimer();
        }
    }

    // Debug
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_healthScalingEffects.GetRangeAtProp(CurrentEnergyProp));
        Gizmos.color = Color.white;
    }
}
