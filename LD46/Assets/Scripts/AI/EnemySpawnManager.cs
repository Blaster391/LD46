using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_enemy = null;
    [SerializeField]
    private float m_spawnRate = 10.0f;
    [SerializeField]
    private int m_maxEnemies = 10;


    private float m_lastSpawnTime = 0.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GameObject.FindObjectsOfType<Enemy>().Length >= m_maxEnemies)
        {
            return;
        }

        m_lastSpawnTime += Time.deltaTime;
        if(m_lastSpawnTime > m_spawnRate)
        {
            m_lastSpawnTime = 0.0f;

            var spawners = GameObject.FindObjectsOfType<EnemySpawner>().Where(x => x.IsAvailable).ToList();
            if (spawners.Count > 0)
            {
                int index = Mathf.RoundToInt(Random.Range(0, spawners.Count));
                SpawnAtSpawner(spawners[index]);
            }
        }
    }

    void SpawnAtSpawner(EnemySpawner spawner)
    {
        var newEnemy = Instantiate<GameObject>(m_enemy);
        newEnemy.transform.position = spawner.transform.position;
    }
}
