using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct PathNode : IPathNodeData
{
    public bool IsWalkable { get; set; }
    public Vector3Int GridPos { get; set; }
    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost { get; set; }
    public int CameFromNodeIndex { get; set; }

    public void CalculateFCost() => FCost = GCost + HCost;
}

public interface IPathNodeData
{
    bool IsWalkable { get; set; }
    Vector3Int GridPos { get; set; }

    int GCost { get; set; }
    int HCost { get; set; }
    int FCost { get; set; }

    int CameFromNodeIndex { get; set; }

    void CalculateFCost();
}