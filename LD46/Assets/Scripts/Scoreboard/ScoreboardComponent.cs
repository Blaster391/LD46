using Newtonsoft.Json;
using ScoreboardCore.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;

namespace Scoreboard
{
    namespace Unity
    {
        public class ScoreboardConnection
        {
            [SerializeField]
            public string DatabaseAddress { get; set; }

            [SerializeField]
            public string GameName { get; set; }

            [SerializeField]
            public string GameKey { get; set; }
        }

        public class ScoreboardComponent : MonoBehaviour
        {
            private void Start()
            {
                m_connection = JsonConvert.DeserializeObject<ScoreboardConnection>(File.ReadAllText(m_scoreboardConnectionFile));

                // For testing
                Score score = new Score();
                score.Level = "Test";
                score.User = "Oscar Biggs";
                score.ScoreValue = 123;
                score.ExtraData.Add("test", "data");

                SubmitResult(score);
            }

            [SerializeField]
            private string m_scoreboardConnectionFile = "";

            private ScoreboardConnection m_connection = null;

            public void GetHighscores(Func<List<ScoreboardCore.Data.ScoreResult>, bool, bool> _onRequestComplete)
            {
                StartCoroutine(GetHighscoresCoroutine(_onRequestComplete));
            }

            private IEnumerator GetHighscoresCoroutine(Func<List<ScoreboardCore.Data.ScoreResult>, bool, bool> _onRequestComplete)
            {

                string getUrl = "/api/scoreboard/" + m_connection.GameName;
                var request = UnityWebRequest.Get(m_connection.DatabaseAddress + getUrl);
                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError(request.error);

                    _onRequestComplete(null, false);
                }
                else
                {
                    Debug.Log("Get request complete!");

                    string resultsString = request.downloadHandler.text;
                    Debug.Log(resultsString);
                    List<ScoreboardCore.Data.ScoreResult> results = JsonConvert.DeserializeObject<List<ScoreboardCore.Data.ScoreResult>>(resultsString);
                    Debug.Log(results.Count);

                    _onRequestComplete(results, false);
                }
            }

            public void SubmitResult(ScoreboardCore.Data.Score score)
            {
                StartCoroutine(SubmitResultCoroutine(score));
            }

            private IEnumerator SubmitResultCoroutine(ScoreboardCore.Data.Score score)
            {

                string submissionUrl = "/api/scoreboard/" + m_connection.GameName + "/submit";

                ScoreSubmissionRequest scoreSubmission = ScoreboardCore.Encrypt.ScoreEncrypt.CreateRequestForScore(score, m_connection.GameKey);
                string scoreText = JsonConvert.SerializeObject(scoreSubmission);
                Debug.Log(scoreText);
                var request = UnityWebRequest.Put(m_connection.DatabaseAddress + submissionUrl, scoreText);
                request.method = UnityWebRequest.kHttpVerbPOST;
                request.SetRequestHeader("content-Type", "application/json");
                request.SetRequestHeader("accept", "text/plain");
  
                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError(request.error);

                }
                else
                {
                    Debug.Log("Post request complete!");

                    string resultsString = request.downloadHandler.text;
                    Debug.Log(resultsString);
                }
            }
        }
    }
}
