using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace UnityBase.PathFinding
{
    public struct FindPath<T> where T : struct, IGridNodeData
    {
        public int3 GridSize;
        public T[] PathNodeArray;
        public Vector3Int StartPos;
        public Vector3Int EndPos;
        public bool AllowDiagonalCornerCutting;

        public List<Vector3Int> Execute()
        {
            var result = new List<Vector3Int>();

            var openList = new List<int>();
            var closedList = new HashSet<int>();

            int startIndex = CalculateIndex(StartPos.x, StartPos.y, StartPos.z);
            int endIndex = CalculateIndex(EndPos.x, EndPos.y, EndPos.z);

            var startNode = PathNodeArray[startIndex];
            startNode.GCost = 0;
            startNode.HCost = CalculateHeuristic(StartPos, EndPos);
            startNode.CalculateFCost();
            PathNodeArray[startIndex] = startNode;

            openList.Add(startIndex);

            var offsets = GetNeighborOffsetsArray(GridSize.z);

            while (openList.Count > 0)
            {
                int currentIndex = GetLowestCostFNodeIndex(openList);
                var currentNode = PathNodeArray[currentIndex];

                if (currentIndex == endIndex)
                    break;

                openList.Remove(currentIndex);
                closedList.Add(currentIndex);

                foreach (var offset in offsets)
                {
                    var neighbourPos = currentNode.GridPos + new Vector3Int(offset.x, offset.y, offset.z);

                    if (!IsPositionInsideGrid(neighbourPos)) continue;

                    if (!AllowDiagonalCornerCutting && math.abs(offset.x) + math.abs(offset.y) + math.abs(offset.z) > 1)
                        if (!IsAxisWalkable(currentNode.GridPos, offset)) continue;

                    int neighbourIndex = CalculateIndex(neighbourPos.x, neighbourPos.y, neighbourPos.z);
                    if (closedList.Contains(neighbourIndex)) continue;

                    var neighbourNode = PathNodeArray[neighbourIndex];
                    if (!neighbourNode.IsWalkable) continue;

                    int axisSum = math.abs(offset.x) + math.abs(offset.y) + math.abs(offset.z);
                    int moveCost = axisSum == 1 ? 10 : 14;
                    int tentativeGCost = currentNode.GCost + moveCost;

                    if (tentativeGCost < neighbourNode.GCost)
                    {
                        neighbourNode.CameFromNodeIndex = currentIndex;
                        neighbourNode.GCost = tentativeGCost;
                        neighbourNode.HCost = CalculateHeuristic(neighbourPos, EndPos);
                        neighbourNode.CalculateFCost();
                        PathNodeArray[neighbourIndex] = neighbourNode;

                        if (!openList.Contains(neighbourIndex))
                            openList.Add(neighbourIndex);
                    }
                }
            }

            ConstructPath(result, endIndex);
            return result;
        }

        private void ConstructPath(List<Vector3Int> pathList, int endIndex)
        {
            var currentNode = PathNodeArray[endIndex];
            if (currentNode.CameFromNodeIndex == -1) return;

            while (true)
            {
                pathList.Add(currentNode.GridPos);
                if (currentNode.CameFromNodeIndex == -1) break;
                currentNode = PathNodeArray[currentNode.CameFromNodeIndex];
            }
        }

        private bool IsAxisWalkable(Vector3Int origin, int3 offset)
        {
            if (math.abs(offset.x) + math.abs(offset.y) + math.abs(offset.z) <= 1) return true;

            if (offset.x != 0)
            {
                var pos = origin + new Vector3Int(offset.x, 0, 0);
                if (!IsPositionInsideGrid(pos) || !PathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].IsWalkable) return false;
            }
            if (offset.y != 0)
            {
                var pos = origin + new Vector3Int(0, offset.y, 0);
                if (!IsPositionInsideGrid(pos) || !PathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].IsWalkable) return false;
            }
            if (offset.z != 0)
            {
                var pos = origin + new Vector3Int(0, 0, offset.z);
                if (!IsPositionInsideGrid(pos) || !PathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].IsWalkable) return false;
            }
            return true;
        }

        private List<int3> GetNeighborOffsetsArray(int depth)
        {
            var list = new List<int3>();
            for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && y == 0 && z == 0) continue;
                if (depth == 1 && z != 0) continue;
                list.Add(new int3(x, y, z));
            }
            return list;
        }

        private int GetLowestCostFNodeIndex(List<int> openList)
        {
            int bestIndex = openList[0];
            var bestNode = PathNodeArray[bestIndex];

            for (int i = 1; i < openList.Count; i++)
            {
                int index = openList[i];
                var node = PathNodeArray[index];
                if (node.FCost < bestNode.FCost)
                {
                    bestIndex = index;
                    bestNode = node;
                }
            }
            return bestIndex;
        }

        private int CalculateIndex(int x, int y, int z)
            => x + y * GridSize.x + z * GridSize.x * GridSize.y;

        private bool IsPositionInsideGrid(Vector3Int pos)
            => pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
               pos.x < GridSize.x && pos.y < GridSize.y && pos.z < GridSize.z;
        
        private int CalculateHeuristic(Vector3Int from, Vector3Int to)
        {
            var dx = math.abs(from.x - to.x);
            var dy = math.abs(from.y - to.y);
            var dz = math.abs(from.z - to.z);
            var min = math.min(dx, math.min(dy, dz));
            var max = math.max(dx, math.max(dy, dz));
            var mid = dx + dy + dz - min - max;
            return 14 * min + 10 * (mid + max - min);
        }
        
        private int CalculateHeuristicHex(Vector3Int from, Vector3Int to)
        {
            var dx = from.x - to.x;
            var dy = from.y - to.y;
            var dz = -dx - dy;
            return (math.abs(dx) + math.abs(dy) + math.abs(dz)) / 2 * 10;
        }
    }
}