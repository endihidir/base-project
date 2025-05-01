using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityBase.GridSystem;

namespace UnityBase.PathFinding
{
    public class PathFinding
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        private readonly IWorldGrid<PathNode> _grid;
        public PathFinding(IWorldGrid<PathNode> grid) => _grid = grid;

        public NativeList<Vector3Int> FindPathJobs(Vector3Int startPos, Vector3Int endPos, bool allowDiagonalCornerCutting = false)
        {
            NativeArray<PathNode> pathNodeArray = RefillNodeArray(endPos);

            NativeList<Vector3Int> calculatedPath = new NativeList<Vector3Int>(Allocator.TempJob);

            FindPathJob findPathJob = new FindPathJob
            {
                pathNodeArray = pathNodeArray,
                gridSize = new int3(_grid.Width, _grid.Height, _grid.Depth),
                startPos = startPos,
                endPos = endPos,
                calculatedPathList = calculatedPath,
                allowDiagonalCornerCutting = allowDiagonalCornerCutting
            };

            JobHandle fpHandle = findPathJob.Schedule();

            fpHandle.Complete();

            pathNodeArray.Dispose();

            return calculatedPath;
        }

        private NativeArray<PathNode> RefillNodeArray(Vector3Int endNodePos)
        {
            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(_grid.Width * _grid.Height * _grid.Depth, Allocator.TempJob);

            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    for (int z = 0; z < _grid.Depth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        var node = _grid.GetFirst(pos);

                        PathNode pathNode = new PathNode
                        {
                            gridPos = pos,
                            gCost = int.MaxValue,
                            hCost = CalculateDistanceCost(pos, endNodePos),
                            isWalkable = node.isWalkable,
                            cameFromNodeIndex = -1
                        };

                        pathNode.CalculateFCost();
                        pathNodeArray[CalculateIndex(x, y, z, _grid.Width, _grid.Height)] = pathNode;
                    }
                }
            }

            return pathNodeArray;
        }

        private static int CalculateDistanceCost(Vector3Int aPos, Vector3Int bPos)
        {
            int xDistance = math.abs(aPos.x - bPos.x);
            int yDistance = math.abs(aPos.y - bPos.y);
            int zDistance = math.abs(aPos.z - bPos.z);
            int remaining = math.abs(xDistance - yDistance - zDistance);
            return MOVE_DIAGONAL_COST * math.min(math.min(xDistance, yDistance), zDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private static int CalculateIndex(int x, int y, int z, int width, int height)
        {
            return x + y * width + z * width * height;
        }

        [BurstCompile]
        private struct FindPathJob : IJob
        {
            public int3 gridSize;
            public NativeArray<PathNode> pathNodeArray;
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

                var startNode = pathNodeArray[startIndex];
                startNode.gCost = 0;
                startNode.CalculateFCost();
                pathNodeArray[startIndex] = startNode;

                openList.Add(startIndex);

                while (openList.Length > 0)
                {
                    int currentIndex = GetLowestCostFNodeIndex(openList);
                    var currentNode = pathNodeArray[currentIndex];

                    if (currentIndex == endIndex) break;

                    int openIndex = openList.IndexOf(currentIndex);
                    if (openIndex >= 0) openList.RemoveAtSwapBack(openIndex);
                    closedList.Add(currentIndex);

                    for (int i = 0; i < neighbourOffsetArray.Length; i++)
                    {
                        var offset = neighbourOffsetArray[i];
                        var neighbourPos = currentNode.gridPos + new Vector3Int(offset.x, offset.y, offset.z);

                        if (!IsPositionInsideGrid(neighbourPos)) continue;

                        if (!allowDiagonalCornerCutting && math.abs(offset.x) + math.abs(offset.y) + math.abs(offset.z) > 1)
                        {
                            if (!IsAxisWalkable(currentNode.gridPos, offset)) continue;
                        }

                        int neighbourIndex = CalculateIndex(neighbourPos.x, neighbourPos.y, neighbourPos.z);
                        if (closedList.Contains(neighbourIndex)) continue;

                        var neighbourNode = pathNodeArray[neighbourIndex];
                        if (!neighbourNode.isWalkable) continue;

                        int axisSum = math.abs(offset.x) + math.abs(offset.y) + math.abs(offset.z);
                        int moveCost = axisSum == 1 ? MOVE_STRAIGHT_COST : MOVE_DIAGONAL_COST;
                        int tentativeGCost = currentNode.gCost + moveCost;

                        if (tentativeGCost < neighbourNode.gCost)
                        {
                            neighbourNode.cameFromNodeIndex = currentIndex;
                            neighbourNode.gCost = tentativeGCost;
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
                var currentNode = pathNodeArray[endIndex];
                if (currentNode.cameFromNodeIndex == -1) return;

                while (true)
                {
                    calculatedPathList.Add(currentNode.gridPos);
                    if (currentNode.cameFromNodeIndex == -1) break;
                    currentNode = pathNodeArray[currentNode.cameFromNodeIndex];
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
                    if (!pathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].isWalkable) return false;
                }
                if (offset.y != 0)
                {
                    var pos = origin + new Vector3Int(0, offset.y, 0);
                    if (!IsPositionInsideGrid(pos)) return false;
                    if (!pathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].isWalkable) return false;
                }
                if (offset.z != 0)
                {
                    var pos = origin + new Vector3Int(0, 0, offset.z);
                    if (!IsPositionInsideGrid(pos)) return false;
                    if (!pathNodeArray[CalculateIndex(pos.x, pos.y, pos.z)].isWalkable) return false;
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
                PathNode bestNode = pathNodeArray[bestIndex];

                for (int i = 1; i < openList.Length; i++)
                {
                    var index = openList[i];
                    var node = pathNodeArray[index];
                    if (node.fCost < bestNode.fCost)
                    {
                        bestIndex = index;
                        bestNode = node;
                    }
                }

                return bestIndex;
            }
        }
    }
}