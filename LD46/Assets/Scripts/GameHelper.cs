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

    public static bool IsWithinThreshold(Vector2 pos1, Vector2 pos2, float threshold)
    {
        return (pos1 - pos2).magnitude < threshold;
    }

    public static bool HasLineOfSight(GameObject from, GameObject to)
    {
        int layerMask = 1 << 8;
        Vector2 fromPosition2D = from.transform.position;
        Vector2 toPosition2D = to.transform.position;
        Vector2 direction = toPosition2D - fromPosition2D;
        float distance = (fromPosition2D - toPosition2D).magnitude;
        RaycastHit2D result = Physics2D.Raycast(fromPosition2D, direction, distance, layerMask);
        if (result)
        {
            return (result.collider.gameObject == to);
        }
        else
        {
            return true;
        }
    }

    public static bool HasLineOfSight(GameObject from, Vector2 to)
    {
        int layerMask = 1 << 8;
        Vector2 fromPosition2D = from.transform.position;
        float distance = (fromPosition2D - to).magnitude;
        RaycastHit2D result = Physics2D.Raycast(fromPosition2D, to, distance, layerMask);
        if (result)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
