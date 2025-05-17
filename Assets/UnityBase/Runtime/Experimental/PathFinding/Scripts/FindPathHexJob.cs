using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UnityBase.GridSystem
{
    [BurstCompile]
    public struct FindPathHexJob<TNode> : IJob where TNode : struct, IGridNodeData
    {
        public int3 GridSize;
        public NativeArray<TNode> PathNodeArray;
        public Vector3Int StartPos, EndPos;
        public NativeList<Vector3Int> CalculatedPathList;
        public bool IsPointyTopped, AllowDiagonalCornerCutting;

        public void Execute()
        {
            var openList = new NativeList<int>(Allocator.Temp);
            var closedList = new NativeList<int>(Allocator.Temp);

            int startIndex = CalculateIndex(StartPos.x, StartPos.y, StartPos.z);
            int endIndex = CalculateIndex(EndPos.x, EndPos.y, EndPos.z);

            TNode startNode = PathNodeArray[startIndex];
            startNode.GCost = 0;
            startNode.HCost = CalculateHexHeuristic(StartPos, EndPos);
            startNode.CalculateFCost();
            PathNodeArray[startIndex] = startNode;

            openList.Add(startIndex);

            while (openList.Length > 0)
            {
                int currentIndex = GetLowestCostFNodeIndex(openList);
                TNode currentNode = PathNodeArray[currentIndex];

                if (currentIndex == endIndex) break;

                openList.RemoveAtSwapBack(openList.IndexOf(currentIndex));
                closedList.Add(currentIndex);

                var neighborOffsets = GetHexNeighborOffsets(currentNode.GridPos.x, currentNode.GridPos.y, IsPointyTopped);

                foreach (var offset in neighborOffsets)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (GridSize.z == 1 && dz != 0) continue;

                        var neighborPos = currentNode.GridPos + new Vector3Int(offset.x, offset.y, dz);
                        if (!IsPositionInsideGrid(neighborPos)) continue;
                        
                        if (!AllowDiagonalCornerCutting && !IsAxisWalkable(currentNode.GridPos, offset.x, offset.y, dz)) 
                            continue;

                        int neighborIndex = CalculateIndex(neighborPos.x, neighborPos.y, neighborPos.z);
                        if (closedList.Contains(neighborIndex)) continue;

                        TNode neighborNode = PathNodeArray[neighborIndex];
                        if (!neighborNode.IsWalkable) continue;

                        int moveCost = (math.abs(offset.x) + math.abs(offset.y) + math.abs(dz)) == 1 ? 10 : 14;
                        int tentativeGCost = currentNode.GCost + moveCost;

                        if (tentativeGCost < neighborNode.GCost)
                        {
                            neighborNode.CameFromNodeIndex = currentIndex;
                            neighborNode.GCost = tentativeGCost;
                            neighborNode.HCost = CalculateHexHeuristic(neighborPos, EndPos);
                            neighborNode.CalculateFCost();
                            PathNodeArray[neighborIndex] = neighborNode;

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

        private NativeArray<Vector3Int> GetHexNeighborOffsets(int x, int y, bool pointyTopped)
        {
            bool isEven = pointyTopped ? y % 2 == 0 : x % 2 == 0;
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

        private bool IsAxisWalkable(Vector3Int origin, int dx, int dy, int dz)
        {
            int axisCount = math.abs(dx) + math.abs(dy) + math.abs(dz);
            if (axisCount <= 1) return true;

            if (dx != 0)
            {
                var pos = origin + new Vector3Int(dx, 0, 0);
                if (!IsPositionInsideGrid(pos) || !PathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].IsWalkable)
                    return false;
            }

            if (dy != 0)
            {
                var pos = origin + new Vector3Int(0, dy, 0);
                if (!IsPositionInsideGrid(pos) || !PathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].IsWalkable)
                    return false;
            }

            if (dz != 0)
            {
                var pos = origin + new Vector3Int(0, 0, dz);
                if (!IsPositionInsideGrid(pos) || !PathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].IsWalkable)
                    return false;
            }

            return true;
        }

        private void ConstructPath(int endIndex)
        {
            TNode currentNode = PathNodeArray[endIndex];
            if (currentNode.CameFromNodeIndex == -1) return;

            while (true)
            {
                CalculatedPathList.Add(currentNode.GridPos);
                if (currentNode.CameFromNodeIndex == -1) break;
                currentNode = PathNodeArray[currentNode.CameFromNodeIndex];
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
            TNode bestNode = PathNodeArray[bestIndex];

            for (int i = 1; i < openList.Length; i++)
            {
                int index = openList[i];
                TNode node = PathNodeArray[index];
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
            return x + y * GridSize.x + z * GridSize.x * GridSize.y;
        }

        private bool IsPositionInsideGrid(Vector3Int pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
                   pos.x < GridSize.x && pos.y < GridSize.y && pos.z < GridSize.z;
        }
    }
}