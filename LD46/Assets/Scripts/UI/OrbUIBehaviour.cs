using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrbUIBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image m_healthImage;

    [SerializeField] private GameObject m_infoTextAreaObject;
    [SerializeField] private Text m_infoText;

    [SerializeField] private GameObject m_warningTextAreaObject;
    [SerializeField] private Text m_warningText;

    [Header("Text")]
    [SerializeField] private string m_openShop;
    [SerializeField] private string m_lowEnergyWarning;
    [SerializeField] private float m_lowHealthEnergyProp;

    [Header("TutorialyThings")]
    [SerializeField] private int m_timesForceShowShopInfo = 1;

    private OrbBehaviour m_orb;
    private InteractionObject m_orbInteraction;

    private int m_currentTimesForceShownShopInfo = 0;

    public void Initialise(OrbBehaviour _orb)
    {
        m_orb = _orb;
        m_orbInteraction = m_orb.gameObject.GetComponent<InteractionObject>();

        m_orbInteraction.PickedUp += OnPickedUp;
        m_orbInteraction.Dropped += OnDropped;
    }

    private void Update()
    {
        m_healthImage.fillAmount = m_orb.CurrentEnergyProp;
        if(m_orb.CurrentEnergyProp <= m_lowHealthEnergyProp && !m_warningTextAreaObject.activeSelf)
        {
            OpenWarning(m_lowEnergyWarning);
        }
        else if (m_orb.CurrentEnergyProp > m_lowHealthEnergyProp && m_warningTextAreaObject.activeSelf)
        {
            CloseWarning();
        }

        // Position on the orb
        transform.position = Camera.main.WorldToScreenPoint(m_orb.transform.position);
    }

    private void OnPickedUp(PlayerInteraction _playerInteraction)
    {
        if(m_currentTimesForceShownShopInfo < m_timesForceShowShopInfo)
        {
            ++m_currentTimesForceShownShopInfo;

            OpenInfo(m_openShop);
        }
        else if(m_orb.CurrentEnergyProp == 1f)
        {
            OpenInfo(m_openShop);
        }
    }

    private void OnDropped(PlayerInteraction _playerInteraction)
    {
        CloseInfo();
    }

    // Info
    private void OpenInfo(string _infoText)
    {
        m_infoTextAreaObject.SetActive(true);
        m_infoText.text = _infoText;
    }

    private void CloseInfo()
    {
        m_infoTextAreaObject.SetActive(false);
    }

    // Warning
    private void OpenWarning(string _warningText)
    {
        m_warningTextAreaObject.SetActive(true);
        m_warningText.text = _warningText;
    }

    private void CloseWarning()
    {
        m_warningTextAreaObject.SetActive(false);
    }
}
