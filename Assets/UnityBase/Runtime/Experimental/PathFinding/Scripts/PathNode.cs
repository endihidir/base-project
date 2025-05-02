using System;
using Unity.Mathematics;
using UnityBase.GridSystem;
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
    public int CalculateHeuristic(Vector3Int from, Vector3Int to)
    {
        int dx = math.abs(from.x - to.x);
        int dy = math.abs(from.y - to.y);
        int dz = math.abs(from.z - to.z);
        int min = math.min(dx, math.min(dy, dz));
        int max = math.max(dx, math.max(dy, dz));
        int mid = dx + dy + dz - min - max;
        return 14 * min + 10 * (mid + max - min);
    }
}

public interface IPathNodeData
{
    bool IsWalkable { get; }
    Vector3Int GridPos { get; set; }

    int GCost { get; set; }
    int HCost { get; set; }
    int FCost { get; set; }

    int CameFromNodeIndex { get; set; }

    void CalculateFCost();

    int CalculateHeuristic(Vector3Int from, Vector3Int to);
}