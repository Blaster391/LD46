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
        if(m_alive)
        {
            EnergyLeached += energy;
        }

    }

    public void IncrementKills()
    {
        if (m_alive)
        {
            Kills++;
        }
    }

    public void AddScore(int amount)
    {
        if (m_alive)
        {
            Score += amount;
        }
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
