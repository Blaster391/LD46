using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField]
    private GameObject m_camera = null;

    [SerializeField]
    private float m_movementSpeed = 1.0f;

    [SerializeField]
    private float m_movementMax = 100.0f;

    float m_movementAmount = 0.0f;
    bool m_reversed = false;

    // Update is called once per frame
    void Update()
    {
        if(m_movementAmount > m_movementMax)
        {
            m_reversed = true;
        }
        else if(-m_movementAmount < m_movementMax)
        {
            m_reversed = false;
        }

        if(m_reversed)
        {
            m_movementAmount += m_movementSpeed * Time.deltaTime;
        }
        else
        {
            m_movementAmount -= m_movementSpeed * Time.deltaTime;
        }

        m_camera.transform.position = new Vector3(m_movementAmount, 0, -10);
    }

    public void EasyLevel()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void MediumLevel()
    {
        SceneManager.LoadScene(2, LoadSceneMode.Single);
    }

    public void HardLevel()
    {
        SceneManager.LoadScene(3, LoadSceneMode.Single);
    }

    public void TutorialLevel()
    {
        SceneManager.LoadScene(4, LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
