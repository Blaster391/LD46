using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIBehaviour : MonoBehaviour
{
    [SerializeField] private ShopDataScriptableObject m_shopData = null;

    [Header("References")]
    [SerializeField] private GameObject m_content = null;
    [SerializeField] private ShopTurretOptionUIBehaviour m_turretOptionPrefab = null;
    [SerializeField] private Text m_energyText = null;

    OrbBehaviour m_orb;
    
    public void Initialise(OrbBehaviour _orb)
    {
        m_orb = _orb;

        // Remove all children we've set up for visualisation in the editor
        for (int i = m_content.transform.childCount - 1; i >= 0; --i)
        {
            Destroy(m_content.transform.GetChild(i).gameObject);
        }

        foreach(ShopOption shopOption in m_shopData.m_options)
        {
            ShopTurretOptionUIBehaviour turretOptionUI = Instantiate(m_turretOptionPrefab, m_content.transform);
            turretOptionUI.Initialise(shopOption, _orb);
        }
    }

    private void Update()
    {
        m_energyText.text = $"E: {Mathf.FloorToInt(m_orb.CurrentEnergy).ToString()}";
    }
}
