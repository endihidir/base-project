using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UnityBase.PathFinding
{
    [BurstCompile]
    public struct FindPathJob<TNode> : IJob where TNode : struct, IGridNodeData
    {
        public int3 gridSize;
        public NativeArray<TNode> pathNodeArray;
        public Vector3Int startPos, endPos;
        public NativeList<Vector3Int> calculatedPathList;
        public bool allowDiagonalCornerCutting;

        public void Execute()
        {
            var neighbourOffsetArray = GetNeighborOffsetsArray(gridSize.z);
            var openList = new NativeList<int>(Allocator.Temp);
            var closedList = new NativeList<int>(Allocator.Temp);

            int startIndex = CalculateIndex(startPos.x, startPos.y, startPos.z);
            int endIndex = CalculateIndex(endPos.x, endPos.y, endPos.z);

            TNode startNode = pathNodeArray[startIndex];
            startNode.GCost = 0;
            startNode.HCost = CalculateHeuristic(startPos, endPos);
            startNode.CalculateFCost();
            pathNodeArray[startIndex] = startNode;

            openList.Add(startIndex);

            while (openList.Length > 0)
            {
                int currentIndex = GetLowestCostFNodeIndex(openList);
                TNode currentNode = pathNodeArray[currentIndex];

                if (currentIndex == endIndex) break;

                int openIndex = openList.IndexOf(currentIndex);
                if (openIndex >= 0) openList.RemoveAtSwapBack(openIndex);
                closedList.Add(currentIndex);

                for (int i = 0; i < neighbourOffsetArray.Length; i++)
                {
                    var offset = neighbourOffsetArray[i];
                    var neighbourPos = currentNode.GridPos + new Vector3Int(offset.x, offset.y, offset.z);

                    if (!IsPositionInsideGrid(neighbourPos)) continue;

                    if (!allowDiagonalCornerCutting && math.abs(offset.x) + math.abs(offset.y) + math.abs(offset.z) > 1)
                    {
                        if (!IsAxisWalkable(currentNode.GridPos, offset)) continue;
                    }

                    int neighbourIndex = CalculateIndex(neighbourPos.x, neighbourPos.y, neighbourPos.z);
                    if (closedList.Contains(neighbourIndex)) continue;

                    TNode neighbourNode = pathNodeArray[neighbourIndex];
                    if (!neighbourNode.IsWalkable) continue;

                    int axisSum = math.abs(offset.x) + math.abs(offset.y) + math.abs(offset.z);
                    int moveCost = axisSum == 1 ? 10 : 14;
                    int tentativeGCost = currentNode.GCost + moveCost;

                    if (tentativeGCost < neighbourNode.GCost)
                    {
                        neighbourNode.CameFromNodeIndex = currentIndex;
                        neighbourNode.GCost = tentativeGCost;
                        neighbourNode.HCost = CalculateHeuristic(neighbourPos, endPos);
                        neighbourNode.CalculateFCost();
                        pathNodeArray[neighbourIndex] = neighbourNode;

                        if (!openList.Contains(neighbourIndex))
                        {
                            openList.Add(neighbourIndex);
                        }
                    }
                }
            }

            ConstructPath(endIndex);

            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
        }

        private void ConstructPath(int endIndex)
        {
            TNode currentNode = pathNodeArray[endIndex];
            if (currentNode.CameFromNodeIndex == -1) return;

            while (true)
            {
                calculatedPathList.Add(currentNode.GridPos);
                if (currentNode.CameFromNodeIndex == -1) break;
                currentNode = pathNodeArray[currentNode.CameFromNodeIndex];
            }
        }

        private NativeArray<int3> GetNeighborOffsetsArray(int depth)
        {
            var list = new NativeList<int3>(Allocator.Temp);
            for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && y == 0 && z == 0) continue;
                if (depth == 1 && z != 0) continue;
                list.Add(new int3(x, y, z));
            }
            var array = new NativeArray<int3>(list.Length, Allocator.Temp);
            NativeArray<int3>.Copy(list.AsArray(), array);
            list.Dispose();
            return array;
        }

        private bool IsAxisWalkable(Vector3Int origin, int3 offset)
        {
            if (math.abs(offset.x) + math.abs(offset.y) + math.abs(offset.z) <= 1) return true;

            if (offset.x != 0)
            {
                var pos = origin + new Vector3Int(offset.x, 0, 0);
                if (!IsPositionInsideGrid(pos)) return false;
                if (!pathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].IsWalkable) return false;
            }
            if (offset.y != 0)
            {
                var pos = origin + new Vector3Int(0, offset.y, 0);
                if (!IsPositionInsideGrid(pos)) return false;
                if (!pathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].IsWalkable) return false;
            }
            if (offset.z != 0)
            {
                var pos = origin + new Vector3Int(0, 0, offset.z);
                if (!IsPositionInsideGrid(pos)) return false;
                if (!pathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].IsWalkable) return false;
            }

            return true;
        }

        private int CalculateIndex(int x, int y, int z)
        {
            return x + y * gridSize.x + z * gridSize.x * gridSize.y;
        }

        private bool IsPositionInsideGrid(Vector3Int pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
                   pos.x < gridSize.x && pos.y < gridSize.y && pos.z < gridSize.z;
        }

        private int GetLowestCostFNodeIndex(NativeList<int> openList)
        {
            int bestIndex = openList[0];
            TNode bestNode = pathNodeArray[bestIndex];

            for (int i = 1; i < openList.Length; i++)
            {
                int index = openList[i];
                TNode node = pathNodeArray[index];
                if (node.FCost < bestNode.FCost)
                {
                    bestIndex = index;
                    bestNode = node;
                }
            }

            return bestIndex;
        }
        
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
    }
}