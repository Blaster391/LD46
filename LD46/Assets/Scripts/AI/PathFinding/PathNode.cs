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
}
