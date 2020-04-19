using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_endGamePrefab = null;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGameOver()
    {
        GameHelper.GetManager<StatsManager>().EndAliveTimer();

        var gameOver = Instantiate<GameObject>(m_endGamePrefab, FindObjectOfType<Canvas>().transform);
       
       // gameOver.transform.position = gameOver.transform.parent.position;
    }
}
