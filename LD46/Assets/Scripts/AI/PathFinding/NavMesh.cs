﻿using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

public class NavMesh : MonoBehaviour
{
    private class CalculatingNode
    {
        public float TotalCost
        {
            get
            {
                return DistanceFromStart + DistanceToEnd;
            }
        }
        public float DistanceFromStart = -1.0f;
        public float DistanceToEnd = -1.0f;
        public PathNode Node = null;
        public CalculatingNode Parent = null;
        // Equals and Hash only care about Node
        //public override bool Equals(object obj)
        //{
        //    if (!(obj is CalculatingNode))
        //    {
        //        return false;
        //    }

        //    var node = (CalculatingNode)obj;
        //    return EqualityComparer<PathNode>.Default.Equals(Node, node.Node);
        //}

        //public override int GetHashCode()
        //{
        //    return -56134859 + EqualityComparer<PathNode>.Default.GetHashCode(Node);
        //}
    }

    [SerializeField]
    private float _requiredRadius = 0.25f;
    [SerializeField]
    private float _spacing = 0.25f;
    [SerializeField]
    private int _maxVerticalNodes = 100;
    [SerializeField]
    private int _maxHorizontalNodes = 100;
    [SerializeField]
    private string m_navmeshFile = "";

    [SerializeField]
    private bool m_generateDiagonals = false;

    [SerializeField]
    private bool m_debugDraw = true;



    private Vector2 _origin = new Vector2(0, 0);

    // private List<PathNode> _nodes = new List<PathNode>();
    private Dictionary<Vector2Int, PathNode> _nodeMap = new Dictionary<Vector2Int, PathNode>();

    // Start is called before the first frame update
    void Start()
    {
        if(m_navmeshFile == "")
        {
            GeneratePathNodes(gameObject.transform.position);
        }
        else
        {
            LoadNavmeshFromFile();
        }
       // GameHelper.GetManager<PathManager>().Register(this);
    }


    const int maxIterations = 1000;
    List<CalculatingNode> m_openList = new List<CalculatingNode>(maxIterations);
    List<CalculatingNode> m_closedList = new List<CalculatingNode>(maxIterations);
    // Querying
    public List<Vector2> RequestPath(Vector2 from, Vector2 to)
    {
        m_openList.Clear();
        m_closedList.Clear();

        Vector2Int startingIndex = GetNearestIndexValid(from);
        Vector2Int endingIndex = GetNearestIndexValid(to);

        if (startingIndex == endingIndex)
        {
            return new List<Vector2> { from, to };
        }

        if (!_nodeMap.ContainsKey(startingIndex))
        {
            return null;
        }
        if (!_nodeMap.ContainsKey(endingIndex))
        {
            return null;
        }

        PathNode startingNode = _nodeMap[startingIndex];
        PathNode endingNode = _nodeMap[endingIndex];

        _debugLastPathStartPoint = startingNode.Position;
        _debugLastPathEndPoint = endingNode.Position;


        CalculatingNode currentNode = new CalculatingNode();
        currentNode.Node = startingNode;
        currentNode.DistanceFromStart = 0.0f;
        currentNode.DistanceToEnd = CalculateDistance(currentNode.Node, endingNode);


        int iteration = 0;
        do
        {
            ++iteration;
           
            //Debug.Log(iteration.ToString() + " " + currentNode.Node.Position.ToString());

            // Add to closed list
            m_closedList.Add(currentNode);

            // Add/update connected nodes to open list
            foreach (var connectedNode in currentNode.Node.ConnectedNodes)
            {
                if (m_closedList.Any(x => x.Node == connectedNode))
                {
                    continue;
                }

                CalculatingNode calculatedConnectedNode = new CalculatingNode();
                calculatedConnectedNode.DistanceFromStart = currentNode.DistanceFromStart + CalculateDistance(currentNode.Node, connectedNode);
                calculatedConnectedNode.Node = connectedNode;


                if (m_openList.Any(x => x.Node == connectedNode))
                {
                    CalculatingNode existingNode = m_openList.First(x => x.Node == calculatedConnectedNode.Node);
                    if (existingNode.DistanceFromStart > calculatedConnectedNode.DistanceFromStart)
                    {
                        existingNode.DistanceFromStart = calculatedConnectedNode.DistanceFromStart;
                        existingNode.Parent = currentNode;
                    }

                }
                else
                {
                    calculatedConnectedNode.DistanceToEnd = CalculateDistance(calculatedConnectedNode.Node, endingNode);
                    calculatedConnectedNode.Parent = currentNode;
                    m_openList.Add(calculatedConnectedNode);
                }
            }

            // Sort open list
            m_openList.Sort((a, b) => { return a.TotalCost.CompareTo(b.TotalCost); });

            // Select new current node
            currentNode = m_openList.First();
            m_openList.Remove(currentNode);

           // Debug.Log(openList.Count);
        } while ((currentNode.Node != endingNode) && (m_openList.Count > 0) && (iteration < maxIterations));

        // Check was successful
        if (currentNode.Node != endingNode)
        {
            return null;
        }

        // Build path
        List<Vector2> results = new List<Vector2>(m_openList.Count);
        results.Add(to);
        while(currentNode.Parent != null)
        {
            results.Add(currentNode.Node.Position);
            currentNode = currentNode.Parent;
        }
        results.Add(currentNode.Node.Position);
        //for (int i = closedList.Count - 1; i >= 0; --i)
        //{
        //  //  if(currentNode.Node.ConnectedNodes.Contains(closedList[i].Node))
        //  //  {
        //        results.Add(closedList[i].Node.Position);
        //   //     currentNode = closedList[i];
        //    //}
        //    for(int j = 0; j < i; ++j)
        //    {
        //        if(closedList[j].Node.ConnectedNodes.Contains(closedList[i].Node))
        //        {
        //            i = j;
        //            Debug.Log("Shortcut");
        //            //currentNode = closedList[j];
        //        }
        //    }


        //}
        //
        results.Add(from);
        results.Reverse();
        return results;
    }

    private float CalculateDistance(PathNode fromNode, PathNode toNode)
    {
        if(fromNode == toNode)
        {
            return 0.0f;
        }

        float distance = 0.0f;
        distance = Mathf.Abs(fromNode.Position.x - toNode.Position.x) + Mathf.Abs(fromNode.Position.y - toNode.Position.y);
        return distance;
    }

    private Vector2Int GetNearestIndex(Vector2 position)
    {
        Vector2 offsetFromOrigin = position - _origin;
        Vector2 unroundedIndex = (offsetFromOrigin) / _spacing;
       // Debug.Log(offsetFromOrigin);
        

        Vector2Int index = new Vector2Int(Mathf.RoundToInt(unroundedIndex.x), Mathf.RoundToInt(unroundedIndex.y)) + new Vector2Int(_maxHorizontalNodes / 2, _maxVerticalNodes / 2);
       // Debug.Log(index);
        return index;
    }

    private Vector2Int GetNearestIndexValid(Vector2 position)
    {
        Vector2Int index = GetNearestIndex(position);

        if(_nodeMap.ContainsKey(index))
        {
            return index;
        }

        if(_nodeMap.ContainsKey(index + new Vector2Int(0,1)))
        {
            return index + new Vector2Int(0, 1);
        }

        if (_nodeMap.ContainsKey(index + new Vector2Int(0, -1)))
        {
            return index + new Vector2Int(0, -1);
        }

        if (_nodeMap.ContainsKey(index + new Vector2Int(1, 0)))
        {
            return index + new Vector2Int(1, 0);
        }

        if (_nodeMap.ContainsKey(index + new Vector2Int(-1, 0)))
        {
            return index + new Vector2Int(-1, 0);
        }

        // We couldn't find anything :c
        return index;
    }

    // Generation
    private void GeneratePathNodes(Vector2 origin)
    {
        _origin = origin;
        _nodeMap.Clear();

        // Create nodes
        for (int x = 0; x < _maxHorizontalNodes; ++x)
        {
            for (int y = 0; y < _maxVerticalNodes; ++y)
            {
                float xOffset = (x - _maxHorizontalNodes / 2) * _spacing;
                float yOffset = (y - _maxVerticalNodes / 2) * _spacing;
                Vector2 position = origin + new Vector2(xOffset, yOffset);

                GenerateNode(position, new Vector2Int(x, y));
            }
        }

        // Connect nodes
        for (int x = 0; x < _maxHorizontalNodes; ++x)
        {
            for (int y = 0; y < _maxVerticalNodes; ++y)
            {
                GenerateNodeConnections(new Vector2Int(x, y));
            }
        }


        // Cull unwalkable space
        var unwalkableMarkers = GameObject.FindGameObjectsWithTag("UnwalkableArea");
        foreach(var marker in unwalkableMarkers)
        {
            ClearUnwalkableArea(marker.GetComponent<UnwalkableAreaMarker>());
        }

        // Cull non connected nodes
        for (int x = 0; x < _maxHorizontalNodes; ++x)
        {
            for (int y = 0; y < _maxVerticalNodes; ++y)
            {
                Vector2Int index = new Vector2Int(x, y);
                if (_nodeMap.ContainsKey(index))
                {
                    var node = _nodeMap[index];
                    if (node.ConnectedNodes.Count == 0)
                    {
                        _nodeMap.Remove(index);
                    }
                }
            }
        }
    }

    private void LoadNavmeshFromFile()
    {
        _nodeMap.Clear();

        TextAsset navmeshText = Resources.Load<TextAsset>("Navmesh/" + m_navmeshFile + ".path");
        List<PathNodeSerializable> serializableNodes = JsonConvert.DeserializeObject<List<PathNodeSerializable>>(navmeshText.text);

        foreach(var node in serializableNodes)
        {
            PathNode newNode = new PathNode(node.Position.Get(), node.Index.Get());
            newNode.ConnectedNodes.Capacity = node.ConnectedNodes.Count;
            _nodeMap.Add(node.Index.Get(), newNode);
        }


        foreach (var serializedNode in serializableNodes)
        {
            PathNode realNode = _nodeMap[serializedNode.Index.Get()];
            foreach(var connectedNode in serializedNode.ConnectedNodes)
            {
                if(_nodeMap.ContainsKey(connectedNode.Get()))
                {
                    realNode.AddConnectedNodes(_nodeMap[connectedNode.Get()]);
                }
            }
        }
    }

    public void OutputNavmesh()
    {
        if(m_navmeshFile == "")
        {
            Debug.LogError("GIVE NAVMESH FILE NAME");
            return;
        }

        string file = "Assets/Resources/Navmesh/" + m_navmeshFile + ".path.txt";
        if (File.Exists(file))
        {
            File.Delete(file);
        }

        GeneratePathNodes(gameObject.transform.position);

        List<PathNodeSerializable> serializableNodes = new List<PathNodeSerializable>();
        
        foreach (var node in _nodeMap)
        {
            serializableNodes.Add(node.Value.GetSerializable());
        }

        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream fs = File.Open(file, FileMode.Create);
        //bf.Serialize(fs, serializableNodes);
        //fs.Close();

        string output = JsonConvert.SerializeObject(serializableNodes);
        //Debug.Log(output);
        File.WriteAllText(file, output);
    }

    private void GenerateNode(Vector2 position, Vector2Int index)
    {
        PathNode newNode = new PathNode(position, index);
        _nodeMap.Add(index, newNode);
    }

    private void GenerateNodeConnections(Vector2Int index)
    {
        if (!_nodeMap.ContainsKey(index))
        {
            return;
        }

        PathNode node = _nodeMap[index];

        CheckNodeConnection(node, index + new Vector2Int(-1, 0));
        CheckNodeConnection(node, index + new Vector2Int(0, -1));

        if(m_generateDiagonals)
        {
            CheckNodeConnection(node, index + new Vector2Int(-1, -1));
            CheckNodeConnection(node, index + new Vector2Int(1, -1));
        }

    }

    private void ClearUnwalkableArea(UnwalkableAreaMarker marker)
    {
        Vector2Int nearestIndex = GetNearestIndex(marker.transform.position);

        if(_nodeMap.ContainsKey(nearestIndex))
        {
            RemoveNodeAndChildren(_nodeMap[nearestIndex]);
        }

    }

    private void RemoveNodeAndChildren(PathNode node)
    {
        const int maxRecusions = 500;
        List<PathNode> deletedNodes = new List<PathNode>();
        RemoveNodeAndChildren(node, maxRecusions);
    }
    private void RemoveNodeAndChildren(PathNode node, int maxRecusions)
    {
        --maxRecusions;
        if(maxRecusions < 0)
        {
            return;
        }
        _nodeMap.Remove(node.Index);
        foreach (var child in node.ConnectedNodes)
        {
            if (_nodeMap.ContainsKey(child.Index))
            {
                RemoveNodeAndChildren(child, maxRecusions);
            }
        }
       
    }

    private void CheckNodeConnection(PathNode node, Vector2Int connectionIndex)
    {
        if (!_nodeMap.ContainsKey(connectionIndex))
        {
            return;
        }

        PathNode targetNode = _nodeMap[connectionIndex];

        // Check clear path
        Vector2 direction = targetNode.Position - node.Position;
        float distance = direction.magnitude;
        direction.Normalize();

        RaycastHit2D[] results = new RaycastHit2D[10];
        ContactFilter2D filter = new ContactFilter2D();
        var numberOfHits = Physics2D.CircleCast(node.Position, _requiredRadius, direction, filter, results, distance);
        if (numberOfHits > 0)
        {
            for(int i = 0; i < numberOfHits; ++i)
            {
                if(results[i].transform?.tag == "StaticWorld")
                {
                    return;
                }
            }
        }

        // Connect nodes
        targetNode.AddConnectedNodes(node);
        node.AddConnectedNodes(targetNode);

    }

    //List<Vector2> _debugPath = null;

    Vector2 _debugLastPathStartPoint;
    Vector2 _debugLastPathEndPoint;
    private void OnDrawGizmosSelected()
    {
        if(m_debugDraw)
        {
            DebugDraw();


        }

        Gizmos.DrawSphere(_debugLastPathStartPoint, 1.0f);
        Gizmos.DrawSphere(_debugLastPathEndPoint, 1.0f);

        //if (Input.GetKey(KeyCode.Y))
        //{
        //    _debugPath = RequestPath(new Vector2(0, 0), GameHelper.MouseToWorldPosition());

        //}

        //   DebugDrawPath(_debugPath);
    }

    private void DebugDraw()
    {
        float radius = _requiredRadius;
        foreach (var nodePair in _nodeMap)
        {
            PathNode node = nodePair.Value;
            Gizmos.DrawWireSphere(node.Position, radius);
            foreach (var connectedNode in node.ConnectedNodes)
            {
                Gizmos.DrawLine(node.Position, connectedNode.Position);
            }
        }
    }

    public void DebugDrawPath(List<Vector2> path)
    {
        
        if(path != null && path.Count > 1)
        {
            for(int i = 1; i < path.Count; ++i)
            {
                // Gizmos.DrawWireSphere(list[i], 0.1f);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(path[i - 1], path[i]);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NavMesh))]
public class NavMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {

        base.DrawDefaultInspector();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Navmesh"))
        {
            NavMesh navMesh = (NavMesh)target;
            navMesh.OutputNavmesh();
        }
        GUILayout.EndHorizontal();
    }
}
#endif
