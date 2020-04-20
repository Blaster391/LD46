using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    [SerializeField]
    private float m_killScoreMod = 1.0f;
    [SerializeField]
    private float m_timeScoreMod = 0.1f;
    [SerializeField]
    private float m_drainScoreMod = 0.05f;

    public float EnergyLeached { get; private set; } = 0;
    public int Score 
    { 
        get
        {
            return Mathf.RoundToInt(Kills * m_killScoreMod + TimeAlive * m_timeScoreMod + EnergyLeached * m_drainScoreMod);
        }
    } 
    public int Kills { get; private set; } = 0;
    public float TimeAlive { get; private set; } = 0;

    public bool IsAlive { get; private set; } = false;

    public string Level { get { return m_level; } }

    [SerializeField]
    private string m_level = "";

    public void AddLeachedEnergy(float energy)
    {
        if(IsAlive)
        {
            EnergyLeached += energy;
        }

    }

    public void IncrementKills()
    {
        if (IsAlive)
        {
            Kills++;
        }
    }

    public void StartAliveTimer()
    {
        IsAlive = true;
        TimeAlive = 0;
    }

    public void EndAliveTimer()
    {
        IsAlive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(IsAlive)
        {
            TimeAlive += Time.deltaTime;
        }
 
    }
}
