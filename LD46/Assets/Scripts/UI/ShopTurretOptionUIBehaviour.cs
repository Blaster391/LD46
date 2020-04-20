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
    private GameObject m_shopItemPrefab = null;

    [SerializeField] private Button m_button = null;

    private OrbBehaviour m_orbBehavior;
    private ShopOption m_shopOption;

    public AK.Wwise.Event MyEvent;

    public void Initialise(ShopOption shopOption, OrbBehaviour orb)
    {
        m_titleText.text = shopOption.m_title;
        m_subtitleText.text = shopOption.m_subtitle;
        m_energyCostText.text = shopOption.m_energyCost.ToString() + "E";
        m_iconImage.sprite = shopOption.m_sprite;

        m_shopItemPrefab = shopOption.m_shopItemPrefab;

        m_button.onClick.AddListener(ButtonClicked);

        m_orbBehavior = orb;
        m_shopOption = shopOption;
    }

    private void Update()
    {
        if (!InStock())
        {
            m_subtitleText.text = "OUT OF STOCK";
        }
        else
        {
            m_subtitleText.text = m_shopOption.m_subtitle + $" ({m_orbBehavior.GetPurchasedAmount(m_shopItemPrefab)}/{m_shopOption.m_itemLimit})";
        }

        if (CanBuy())
        {
            m_button.image.color = Color.white;
        }
        else
        {

            m_button.image.color = Color.red;
        }
    }

    private bool InStock()
    {
        return m_orbBehavior.GetPurchasedAmount(m_shopItemPrefab) < m_shopOption.m_itemLimit;
    }

    private bool CanBuy()
    {
        if(!InStock())
        {
            return false;
        }

        return (m_orbBehavior.CurrentEnergy > GetCurrentCost() + m_orbBehavior.ShopEnergyBuffer);
    }

    private void ButtonClicked()
    {
        if(CanBuy())
        {
            MyEvent.Post(gameObject);
            m_orbBehavior.PurchaseItem(m_shopItemPrefab, GetCurrentCost());
            m_energyCostText.text = GetCurrentCost().ToString() + "E";
        }
    }

    private float GetCurrentCost()
    {
        return m_shopOption.m_energyCost + (m_orbBehavior.GetPurchasedAmount(m_shopItemPrefab) * m_shopOption.m_energyIncreasePerPurchase);
    }
}
