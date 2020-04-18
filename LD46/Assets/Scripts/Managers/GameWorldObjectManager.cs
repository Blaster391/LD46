using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWorldObjectManager : MonoBehaviour
{
    [SerializeField] private Transform m_turretsParentObject = null;
    [SerializeField] private Transform m_uiParentObject = null;

    public Transform TurretsParent { get { return m_turretsParentObject; } }
    public Transform UIParent { get { return m_uiParentObject; } }
}
