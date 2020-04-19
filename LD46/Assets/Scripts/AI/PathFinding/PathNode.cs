using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public Vector2Int Index { get; private set; }
    public Vector2 Position { get; private set; }
    public List<PathNode> ConnectedNodes { get; private set; } = new List<PathNode>();

    public PathNode(Vector2 position, Vector2Int index)
    {
        Position = position;
        Index = index;
    }

    public void AddConnectedNodes(PathNode node)
    {
        ConnectedNodes.Add(node);
    }

    public PathNodeSerializable GetSerializable()
    {
        PathNodeSerializable node = new PathNodeSerializable();
        node.Index = new Vector2IntSerializable(Index);
        node.Position = new Vector2Serializable(Position);
        foreach(var n in ConnectedNodes)
        {
            node.ConnectedNodes.Add(new Vector2IntSerializable(n.Index));
        }

        return node;
    }
}

[System.Serializable]
public class Vector2IntSerializable
{
    public Vector2IntSerializable()
    {

    }

    public Vector2IntSerializable(Vector2Int val)
    {
        X = val.x;
        Y = val.y;
    }

    public int X { get; set; }
    public int Y { get; set; }

    public Vector2Int Get()
    {
        return new Vector2Int(X, Y);
    }
}

[System.Serializable]
public class Vector2Serializable
{
    public Vector2Serializable()
    {

    }

    public Vector2Serializable(Vector2 val)
    {
        X = val.x;
        Y = val.y;
    }


    public float X { get; set; }
    public float Y { get; set; }

    public Vector2 Get()
    {
        return new Vector2(X, Y);
    }
}


[System.Serializable]
public class PathNodeSerializable
{
    public Vector2IntSerializable Index { get; set; }
    public Vector2Serializable Position { get; set; }
    public List<Vector2IntSerializable> ConnectedNodes { get; set; } = new List<Vector2IntSerializable>();

}