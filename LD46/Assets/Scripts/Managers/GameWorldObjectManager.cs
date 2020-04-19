using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWorldObjectManager : MonoBehaviour
{
    [SerializeField] private Transform m_turretsParentObject = null;
    [SerializeField] private Transform m_enemyParentObject = null;
    [SerializeField] private Transform m_effectsParentObject = null;

    public Transform TurretsParent { get { return m_turretsParentObject; } }
    public Transform EnemyParent { get { return m_enemyParentObject; } }
    public Transform UIParent { get { return FindObjectOfType<Canvas>().transform; } }
    public Transform EffectsParent { get { return m_effectsParentObject; } }
}
