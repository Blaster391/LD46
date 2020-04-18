using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameHelper
{
    public static T GetManager<T>()
    {
        return GameObject.Find("GameMaster").GetComponent<T>();
    }

    public static Vector2 MouseToWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
