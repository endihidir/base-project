using System;
using Unity.Mathematics;

[Serializable]
public struct PathNode
{
    public int x;
    public int y;
    public int index;
    public bool isWalkable;
    
    public int2 Pos => new int2(x,y);

    public int gCost;
    public int hCost;
    public int fCost;
    
    public int cameFromNodeIndex;

    public override string ToString()
    {
        return x + "," + y;
    }

    public void SetWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}