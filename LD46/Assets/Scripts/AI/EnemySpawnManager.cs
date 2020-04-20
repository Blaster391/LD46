using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemySpawnWeight
{
    public Enemy m_enemyPrefab;
    public float m_weight;
}

[System.Serializable]
public class EnemySpawnWaveSetting
{
    public float m_waveDuration = 30f;
    public List<EnemySpawnWeight> m_enemySpawnWeights;
    public float m_spawnRate = 10f;
    public int m_maxEnemies = 10;

    public Enemy GetEnemy()
    {
        float totalWeight = 0;
        foreach(EnemySpawnWeight spawnWeight in m_enemySpawnWeights)
        {
            totalWeight += spawnWeight.m_weight;
        }

        if(totalWeight == 0)
        {
            return null;
        }

        float selectedWeight = Random.Range(0f, totalWeight);

        float currentTotalWeight = 0f;
        foreach (EnemySpawnWeight spawnWeight in m_enemySpawnWeights)
        {
            currentTotalWeight += spawnWeight.m_weight;
            if(selectedWeight < currentTotalWeight)
            {
                return spawnWeight.m_enemyPrefab;
            }
        }
        return null;
    }
}

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField]
    private List<EnemySpawnWaveSetting> m_enemySpawnWaveSettings = new List<EnemySpawnWaveSetting>();

    private GameWorldObjectManager m_gameWorldObjectManager;

    private int m_currentWaveIndex = 0;
    private float m_currentWaveDuration = 0f;

    private float m_lastSpawnTime = 0.0f;

    private void Start()
    {
        m_gameWorldObjectManager = FindObjectOfType<GameWorldObjectManager>();
    }

    void Update()
    {
        m_currentWaveDuration += Time.deltaTime;
        if(m_currentWaveDuration > m_enemySpawnWaveSettings[m_currentWaveIndex].m_waveDuration)
        {
            m_currentWaveDuration = 0.0f;
            // If theres no more wave definitions we'll just keep going with the current one
            if (m_currentWaveIndex + 1 < m_enemySpawnWaveSettings.Count)
            {
                ++m_currentWaveIndex;
            }
        }

        if(Enemy.Enemies.Count >= GetCurrentWave().m_maxEnemies)
        {
            return;
        }

        m_lastSpawnTime += Time.deltaTime;
        if(m_lastSpawnTime > GetCurrentWave().m_spawnRate)
        {
            m_lastSpawnTime = 0.0f;

            var spawners = EnemySpawner.EnemySpawners.Where(x => x.IsAvailable).ToList();
            if (spawners.Count > 0)
            {
                int index = Mathf.RoundToInt(Random.Range(0, spawners.Count));
                SpawnAtSpawner(spawners[index]);
            }
        }
    }

    void SpawnAtSpawner(EnemySpawner spawner)
    {
        Enemy enemyToSpawn = GetEnemy();
        if (enemyToSpawn != null)
        {
            var newEnemy = Instantiate(GetEnemy(), m_gameWorldObjectManager.EnemyParent != null ? m_gameWorldObjectManager.EnemyParent : transform);
            newEnemy.transform.position = spawner.transform.position;
        }
    }

    EnemySpawnWaveSetting GetCurrentWave()
    {
        return m_enemySpawnWaveSettings[m_currentWaveIndex];
    }

    Enemy GetEnemy()
    {
        EnemySpawnWaveSetting waveSetting = GetCurrentWave();
        return waveSetting.GetEnemy();
    }
}
