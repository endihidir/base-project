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

        public NativeList<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos, bool allowDiagonalCornerCutting = false)
        {
            NativeArray<PathNode> pathNodeArray = ResetPathNodeArray(endPos);

            NativeList<Vector3Int> calculatedPath = new NativeList<Vector3Int>(Allocator.TempJob);

            FindPathJob findPathJob = new FindPathJob
            {
                pathNodeArray = pathNodeArray,
                gridSize = new int2(_grid.Width, _grid.Height),
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

        private NativeArray<PathNode> ResetPathNodeArray(Vector3Int endNodePos)
        {
            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(_grid.Width * _grid.Height, Allocator.TempJob);

            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    var pos = new Vector3Int(x, y, 0);

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

                    var index = CalculateIndex(x, y, _grid.Height);
                    pathNodeArray[index] = pathNode;
                }
            }

            return pathNodeArray;
        }

        private static int CalculateDistanceCost(Vector3Int aPos, Vector3Int bPos)
        {
            var xDistance = Mathf.Abs(aPos.x - bPos.x);
            var yDistance = Mathf.Abs(aPos.y - bPos.y);
            var remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private static int CalculateIndex(int x, int y, int gridHeight)
        {
            return y + gridHeight * x;
        }

        [BurstCompile]
        private struct FindPathJob : IJob
        {
            public int2 gridSize;

            public NativeArray<PathNode> pathNodeArray;

            public Vector3Int startPos, endPos;

            public NativeList<Vector3Int> calculatedPathList;
            
            public bool allowDiagonalCornerCutting;

            public void Execute()
            {
                var neighbourOffsetArray = GetNeighBorOffsetsArray();

                var endNodeIndex = CalculateIndex(endPos.x, endPos.y, gridSize.y);

                var startNode = pathNodeArray[CalculateIndex(startPos.x, startPos.y, gridSize.y)];
                startNode.gCost = 0;
                startNode.CalculateFCost();
                pathNodeArray[CalculateIndex(startPos.x, startPos.y, gridSize.y)] = startNode;

                var openList = new NativeList<int>(Allocator.Temp);
                var closedList = new NativeList<int>(Allocator.Temp);

                openList.Add(CalculateIndex(startPos.x, startPos.y, gridSize.y));

                while (openList.Length > 0)
                {
                    var currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);

                    var currentNode = pathNodeArray[currentNodeIndex];

                    if (currentNodeIndex == endNodeIndex) break;

                    for (int i = 0; i < openList.Length; i++)
                    {
                        if (openList[i] == currentNodeIndex)
                        {
                            openList.RemoveAtSwapBack(i);
                            break;
                        }
                    }

                    closedList.Add(currentNodeIndex);

                    for (int i = 0; i < neighbourOffsetArray.Length; i++)
                    {
                        var offset = neighbourOffsetArray[i];
                        var neighbourPos = currentNode.gridPos + new Vector3Int(offset.x, offset.y, 0);

                        if (!IsPositionInsideGrid(neighbourPos, gridSize)) continue;
                        
                        if (!allowDiagonalCornerCutting && math.abs(offset.x) == 1 && math.abs(offset.y) == 1)
                        {
                            var nodeA = currentNode.gridPos + new Vector3Int(offset.x, 0, 0);
                            var nodeB = currentNode.gridPos + new Vector3Int(0, offset.y, 0);

                            if (!IsPositionInsideGrid(nodeA, gridSize) || !IsPositionInsideGrid(nodeB, gridSize)) continue;

                            var indexA = CalculateIndex(nodeA.x, nodeA.y, gridSize.y);
                            var indexB = CalculateIndex(nodeB.x, nodeB.y, gridSize.y);

                            if (!pathNodeArray[indexA].isWalkable || !pathNodeArray[indexB].isWalkable)
                            {
                                continue;
                            }
                        }

                        var neighbourIndex = CalculateIndex(neighbourPos.x, neighbourPos.y, gridSize.y);

                        if (closedList.Contains(neighbourIndex)) continue;

                        var neighbourNode = pathNodeArray[neighbourIndex];
                        if (!neighbourNode.isWalkable) continue;

                        var tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode.gridPos, neighbourNode.gridPos);

                        if (tentativeGCost < neighbourNode.gCost)
                        {
                            neighbourNode.cameFromNodeIndex = currentNodeIndex;
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

                CalculatePath(endNodeIndex);

                neighbourOffsetArray.Dispose();
                openList.Dispose();
                closedList.Dispose();
            }

            private void CalculatePath(int endNodeIndex)
            {
                PathNode endNode = pathNodeArray[endNodeIndex];

                if (endNode.cameFromNodeIndex != -1)
                {
                    calculatedPathList.Add(endNode.gridPos);

                    PathNode currentNode = endNode;

                    while (currentNode.cameFromNodeIndex != -1)
                    {
                        PathNode cameFrom = pathNodeArray[currentNode.cameFromNodeIndex];
                        calculatedPathList.Add(cameFrom.gridPos);
                        currentNode = cameFrom;
                    }
                }
            }

            private NativeArray<int2> GetNeighBorOffsetsArray()
            {
                NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);

                neighbourOffsetArray[0] = new int2(-1, 0);
                neighbourOffsetArray[1] = new int2(+1, 0);
                neighbourOffsetArray[2] = new int2(0, +1);
                neighbourOffsetArray[3] = new int2(0, -1);
                neighbourOffsetArray[4] = new int2(-1, -1);
                neighbourOffsetArray[5] = new int2(-1, +1);
                neighbourOffsetArray[6] = new int2(+1, -1);
                neighbourOffsetArray[7] = new int2(+1, +1);

                return neighbourOffsetArray;
            }

            private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> nodeArray)
            {
                PathNode lowest = nodeArray[openList[0]];

                for (int i = 1; i < openList.Length; i++)
                {
                    PathNode test = nodeArray[openList[i]];
                    if (test.fCost < lowest.fCost)
                    {
                        lowest = test;
                    }
                }

                return lowest.gridPos.y + gridSize.y * lowest.gridPos.x;
            }

            private bool IsPositionInsideGrid(Vector3Int pos, int2 size)
            {
                return pos.x >= 0 && pos.y >= 0 && pos.x < size.x && pos.y < size.y;
            }
        }
    }
}