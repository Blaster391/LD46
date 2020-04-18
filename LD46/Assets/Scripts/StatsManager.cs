using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public float EnergyLeached { get; private set; } = 0;
    public int Score { get; private set; } = 0;
    public int Kills { get; private set; } = 0;
    public float TimeAlive { get; private set; } = 0;

    private bool m_alive = false;
    public void AddLeachedEnergy(float energy)
    {
        EnergyLeached += energy;
    }

    public void IncrementKills()
    {
        Kills++;
    }

    public void AddScore(int amount)
    {
        Score += amount;
    }

    public void StartAliveTimer()
    {
        m_alive = true;
        TimeAlive = 0;
    }

    public void EndAliveTimer()
    {
        m_alive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_alive)
        {
            TimeAlive += Time.deltaTime;
        }
 
    }
}
