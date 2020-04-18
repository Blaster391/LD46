using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBase : MonoBehaviour
{
    public Turret Turret { get; private set; }

    void Awake()
    {
        Turret = GetComponentInChildren<Turret>();
    }
}
