using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitToMainMenuManager : MonoBehaviour
{
    [SerializeField]
    private Text m_exitGameTextPrefab;
    [SerializeField]
    private float m_timeToQuit = 3.0f;


    private Text m_exitToMainMenuText = null;
    private StatsManager m_statsManager = null;
    private bool m_quitting = false;
    private float m_quittingTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_statsManager = GameHelper.GetManager<StatsManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Escape) && m_statsManager.IsAlive)
        {
            if(!m_quitting)
            {
                m_exitToMainMenuText = Instantiate<Text>(m_exitGameTextPrefab, FindObjectOfType<Canvas>().transform);

                m_quittingTime = 0.0f;
                m_quitting = true;
            }

            m_quittingTime += Time.deltaTime;

            m_quittingTime = Mathf.Min(m_timeToQuit, m_quittingTime);

            m_exitToMainMenuText.text = $"Exit to main menu in {(m_timeToQuit - m_quittingTime):0.##}";

            if (m_quittingTime >= m_timeToQuit)
            {
                SceneManager.LoadScene(0, LoadSceneMode.Single);
            }
        }
        else
        {
            if(m_quitting)
            {
                Destroy(m_exitToMainMenuText);
                m_exitToMainMenuText = null;
                m_quitting = false;
            }
        }
    }
}
