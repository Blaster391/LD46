using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopTurretOptionUIBehaviour : MonoBehaviour
{
    [SerializeField] private Text m_titleText = null;
    [SerializeField] private Text m_subtitleText = null;
    [SerializeField] private Text m_energyCostText = null;
    [SerializeField] private Image m_iconImage = null;
    private TurretBase m_turretPrefab = null;

    [SerializeField] private Button m_button = null;

    private OrbBehaviour m_orbBehavior;
    private ShopOption m_shopOption;

    public AK.Wwise.Event MyEvent;

    public void Initialise(ShopOption shopOption, OrbBehaviour orb)
    {
        m_titleText.text = shopOption.m_title;
        m_subtitleText.text = shopOption.m_subtitle;
        m_energyCostText.text = shopOption.m_energyCost.ToString() + "£";
        m_iconImage.sprite = shopOption.m_sprite;

        m_turretPrefab = shopOption.m_turretPrefab;

        m_button.onClick.AddListener(ButtonClicked);

        m_orbBehavior = orb;
        m_shopOption = shopOption;
    }

    private void Update()
    {
        if(m_orbBehavior.CurrentEnergy > m_shopOption.m_energyCost)
        {
            m_button.image.color = Color.white;
        }
        else
        {
            m_button.image.color = Color.red;
        }
    }

    private void ButtonClicked()
    {
        MyEvent.Post(gameObject);
        m_orbBehavior.PurchaseTurret(m_turretPrefab, m_shopOption.m_energyCost);
    }
}
