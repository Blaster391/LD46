using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopOption
{
    public Turret m_turretPrefab;
    public float m_energyCost;
    public string m_title;
    public string m_subtitle;
    public Sprite m_sprite;
}

[CreateAssetMenu(menuName = "LD46/Data/Shop")]
public class ShopDataScriptableObject : ScriptableObject
{
    public List<ShopOption> m_options = new List<ShopOption>();
}
