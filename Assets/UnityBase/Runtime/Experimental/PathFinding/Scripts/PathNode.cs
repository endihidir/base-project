using System;
using Unity.Mathematics;
using UnityBase.GridSystem;
using UnityEngine;

[Serializable]
public struct PathNode
{
    public Vector3Int gridPos;
    public bool isWalkable;

    public int gCost;
    public int hCost;
    public int fCost;
    
    public int cameFromNodeIndex;

    public override string ToString()
    {
        return gridPos.x + "," + gridPos.y + "," + gridPos.z;
    }

    public void CalculateFCost() => fCost = gCost + hCost;
}