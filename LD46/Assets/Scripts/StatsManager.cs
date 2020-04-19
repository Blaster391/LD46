using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{

    public float EnergyLeached { get; private set; } = 0;
    public int Score { get; private set; } = 0;
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

    public void AddScore(int amount)
    {
        if (IsAlive)
        {
            Score += amount;
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
