using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public NavMesh NavMesh { get; private set; }

    public void Register(NavMesh mesh)
    {
        NavMesh = mesh;
    }

}
