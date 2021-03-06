﻿using Scoreboard.Unity;
using ScoreboardCore.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{

    [SerializeField]
    private InputField m_playerName = null;

    [SerializeField]
    private Text m_playerScore = null;
    [SerializeField]
    private Text m_scoreboardText = null;
    [SerializeField]
    private Text m_scoreboardTitleText = null;
    [SerializeField]
    private Text m_submitScoreText = null;

    StatsManager m_playerStats = null;
    ScoreboardComponent m_scoreboardComponent;
    bool m_scoreSubmitted = false;

    public AK.Wwise.State WWiseState;
    public AK.Wwise.Event MyEvent;

    private float m_timeTillUpdate = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_scoreboardComponent = GetComponent<ScoreboardComponent>();
        m_playerStats = GameHelper.GetManager<StatsManager>();

        m_playerName.Select();

        m_scoreboardText.gameObject.SetActive(false);
        m_scoreboardTitleText.gameObject.SetActive(false);

        ShowScore();
        LoadHighscores();

        WWiseState.SetValue();
        MyEvent.Post(gameObject);
    }

    void ShowScore()
    {
        m_playerScore.text = "";
        m_playerScore.text += $"Score: {m_playerStats.Score} \n";
        m_playerScore.text += $"Kills: {m_playerStats.Kills} \n";
        m_playerScore.text += $"Time Alive: {Mathf.RoundToInt(m_playerStats.TimeAlive)} seconds\n";
        m_playerScore.text += $"Energy Leached: {Mathf.RoundToInt(m_playerStats.EnergyLeached)} \n";
    }

    // Update is called once per frame
    void Update()
    {
        m_timeTillUpdate -= Time.deltaTime;
        if (m_timeTillUpdate < 0)
        {
            m_timeTillUpdate = 5.0f;
            LoadHighscores();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }

        if (!m_scoreSubmitted && Input.GetKeyDown(KeyCode.Return))
        {
            SubmitScore();
        }
    }


    void LoadHighscores()
    {


        Func<List<ScoreboardCore.Data.ScoreResult>, bool, bool> callback = (results, success) =>
        {
            if(success)
            {
                m_scoreboardTitleText.text = $"Leaderboard ({m_playerStats.Level})";

                m_scoreboardText.text = "";
                for (int i = 0; i < results.Count; ++i)
                {
                    ScoreResult result = results[i];
                    m_scoreboardText.text += $"{i+1}. {result.Score.User} : {result.Score.ScoreValue} \n";
                }

                m_scoreboardText.gameObject.SetActive(true);
                m_scoreboardTitleText.gameObject.SetActive(true);
            }
            else
            {

            }

            return true;
        };

        m_scoreboardComponent.GetHighscores(callback, m_playerStats.Level);
    }

    void SubmitScore()
    {
        if(m_playerName.text.Trim() != string.Empty)
        {
            m_submitScoreText.text = "Score Submitting, pls wait...";

            Score playerScore = new Score();
            playerScore.User = m_playerName.text.Trim();
            playerScore.Level = m_playerStats.Level;
            playerScore.ScoreValue = m_playerStats.Score;
            m_playerName.gameObject.SetActive(false);

            Func<bool, string, bool> callback = (success, result) =>
            {
                if(success)
                {
                    m_submitScoreText.text = "Score Submitted!";
                    LoadHighscores();
                }
                else
                {
                    m_submitScoreText.text = "There was a problem, please try again";
                    m_scoreSubmitted = false;
                    m_playerName.gameObject.SetActive(true);
                }

                return true;
            };

            m_scoreboardComponent.SubmitResult(playerScore, callback);

            m_scoreSubmitted = true;
        }

    }
}
