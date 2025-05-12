using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UnityBase.GridSystem
{
    [BurstCompile]
    public struct FindPathHexJob<TNode> : IJob where TNode : struct, IPathNodeData
    {
        public int3 gridSize;
        public NativeArray<TNode> pathNodeArray;
        public Vector3Int startPos, endPos;
        public NativeList<Vector3Int> calculatedPathList;
        public bool isPointyTopped;

        public void Execute()
        {
            var openList = new NativeList<int>(Allocator.Temp);
            var closedList = new NativeList<int>(Allocator.Temp);

            int startIndex = CalculateIndex(startPos.x, startPos.y, startPos.z);
            int endIndex = CalculateIndex(endPos.x, endPos.y, endPos.z);

            TNode startNode = pathNodeArray[startIndex];
            startNode.GCost = 0;
            startNode.HCost = CalculateHexHeuristic(startPos, endPos);
            startNode.CalculateFCost();
            pathNodeArray[startIndex] = startNode;

            openList.Add(startIndex);

            while (openList.Length > 0)
            {
                int currentIndex = GetLowestCostFNodeIndex(openList);
                TNode currentNode = pathNodeArray[currentIndex];

                if (currentIndex == endIndex) break;

                openList.RemoveAtSwapBack(openList.IndexOf(currentIndex));
                closedList.Add(currentIndex);

                var neighborOffsets = GetHexNeighborOffsets(currentNode.GridPos.y, isPointyTopped);

                foreach (var offset in neighborOffsets)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        var neighborPos = currentNode.GridPos + new Vector3Int(offset.x, offset.y, dz);
                        if (!IsPositionInsideGrid(neighborPos)) continue;

                        int neighborIndex = CalculateIndex(neighborPos.x, neighborPos.y, neighborPos.z);
                        if (closedList.Contains(neighborIndex)) continue;

                        TNode neighborNode = pathNodeArray[neighborIndex];
                        if (!neighborNode.IsWalkable) continue;

                        int moveCost = 10 + math.abs(dz) * 10;
                        int tentativeGCost = currentNode.GCost + moveCost;

                        if (tentativeGCost < neighborNode.GCost)
                        {
                            neighborNode.CameFromNodeIndex = currentIndex;
                            neighborNode.GCost = tentativeGCost;
                            neighborNode.HCost = CalculateHexHeuristic(neighborPos, endPos);
                            neighborNode.CalculateFCost();
                            pathNodeArray[neighborIndex] = neighborNode;

                            if (!openList.Contains(neighborIndex))
                                openList.Add(neighborIndex);
                        }
                    }
                }
            }

            ConstructPath(endIndex);

            openList.Dispose();
            closedList.Dispose();
        }

        private NativeArray<Vector3Int> GetHexNeighborOffsets(int y, bool pointyTopped)
        {
            bool isEven = y % 2 == 0;
            var offsets = new NativeArray<Vector3Int>(6, Allocator.Temp);

            if (pointyTopped)
            {
                offsets[0] = new Vector3Int(+1, 0, 0);
                offsets[1] = new Vector3Int(-1, 0, 0);
                offsets[2] = new Vector3Int(isEven ? 0 : +1, +1, 0);
                offsets[3] = new Vector3Int(isEven ? -1 : 0, +1, 0);
                offsets[4] = new Vector3Int(isEven ? 0 : +1, -1, 0);
                offsets[5] = new Vector3Int(isEven ? -1 : 0, -1, 0);
            }
            else
            {
                offsets[0] = new Vector3Int(0, +1, 0);
                offsets[1] = new Vector3Int(0, -1, 0);
                offsets[2] = new Vector3Int(+1, isEven ? 0 : +1, 0);
                offsets[3] = new Vector3Int(+1, isEven ? -1 : 0, 0);
                offsets[4] = new Vector3Int(-1, isEven ? 0 : +1, 0);
                offsets[5] = new Vector3Int(-1, isEven ? -1 : 0, 0);
            }

            return offsets;
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

        private int CalculateHexHeuristic(Vector3Int a, Vector3Int b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            int dz = -a.x - a.y - (-b.x - b.y);
            int horizontal = (math.abs(dx) + math.abs(dy) + math.abs(dz)) / 2;

            return 10 * horizontal + 10 * math.abs(a.z - b.z); 
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

        private int CalculateIndex(int x, int y, int z)
        {
            return x + y * gridSize.x + z * gridSize.x * gridSize.y;
        }

        private bool IsPositionInsideGrid(Vector3Int pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
                   pos.x < gridSize.x && pos.y < gridSize.y && pos.z < gridSize.z;
        }
    }
}
