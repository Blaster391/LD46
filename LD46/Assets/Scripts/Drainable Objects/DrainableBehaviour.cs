using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Something that can be drained by THE ORB
public class DrainableBehaviour : MonoBehaviour
{
    [SerializeField] private float m_maxEnergy;
    private bool takingDamage = false;

    public float CurrentEnergy { get; private set; }
    public float MaxEnergy { get { return m_maxEnergy; } }
    public float CurrentEnergyProp { get { return CurrentEnergy / m_maxEnergy; } }

    public AK.Wwise.Event MyEvent;
    public AK.Wwise.RTPC MyRTPC;

    // Callbacks for visuals if we separate
    public System.Action<DrainableBehaviour> EnergyChange = delegate { }; // Energy remaining, energy prop remaining
    public System.Action Dead = delegate { };

    // Static tracking
    private static List<DrainableBehaviour> s_drainables = new List<DrainableBehaviour>();
    public static List<DrainableBehaviour> Drainables { get { return new List<DrainableBehaviour>(s_drainables); } }
    
    void Start()
    {
        s_drainables.Add(this);
        MyEvent.Post(gameObject);
        MyRTPC.SetValue(gameObject, 100);
        CurrentEnergy = m_maxEnergy;
    }

    private void Update()
    {
        if(!takingDamage)
        {
            MyRTPC.SetValue(gameObject, 100);
        }
        takingDamage = false;
    }

    void OnDestroy()
    {
        s_drainables.Remove(this);
    }
    
    // Interface
    public float TakeEnergy(float _energy)
    {
        float previousEnergy = CurrentEnergy;
        CurrentEnergy = Mathf.Clamp(CurrentEnergy - _energy, 0f, m_maxEnergy);
        MyRTPC.SetValue(gameObject,CurrentEnergy);
        EnergyChange(this);
        if(CurrentEnergy == 0f)
        {
            MyEvent.Stop(gameObject);
            Die();
        }
        takingDamage = true;
        return previousEnergy - CurrentEnergy;
    }

    private void Die()
    {
        Dead();

        // Kill self? Maybe not idk yet
        Destroy(gameObject);
    }
}
