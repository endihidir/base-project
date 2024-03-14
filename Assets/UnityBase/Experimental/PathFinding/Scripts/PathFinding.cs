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

        private Grid<PathNode> _grid;

        private int _width;
        private int _height;

        public PathFinding(IGridEntity gridEntity)
        {
            _grid = new Grid<PathNode>(gridEntity);
            
            _width = _grid.Width;
            _height = _grid.Height;


            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var nodeData = new PathNode { x = x, y = y, index = CalculateIndex(x, y, _height), isWalkable = true };
                    
                    _grid.SetGridObject(x, y, nodeData);
                }
            }
            
            // FindPath(int2.zero, int2.zero);
        }

        public Grid<PathNode> GetGrid() => _grid;

        public NativeList<int2> FindPath(int2 startNodePos, int2 endNodePos)
        {
            NativeArray<PathNode> pathNodeArray = ResetPathNodeArray(endNodePos);

            NativeList<int2> calculatedPath = new NativeList<int2>(Allocator.TempJob);

            FindPathJob findPathJob = new FindPathJob()
            {
                pathNodeArray = pathNodeArray,
                gridSize = new int2(_width, _height),
                startPos = startNodePos,
                endPos = endNodePos,
                calculatedPathList = calculatedPath,
            };

            JobHandle fpHandle = findPathJob.Schedule();

            fpHandle.Complete();

            pathNodeArray.Dispose();

            return calculatedPath;
        }

        private NativeArray<PathNode> ResetPathNodeArray(int2 endNodePos)
        {
            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(_width * _height, Allocator.TempJob);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    PathNode pathNode = new PathNode
                    {
                        x = x,
                        y = y,
                        index = CalculateIndex(x, y, _height),
                        gCost = int.MaxValue,
                        hCost = CalculateDistanceCost(new int2(x, y), endNodePos),
                        isWalkable = _grid.GetGridObject(x, y).isWalkable,
                        cameFromNodeIndex = -1
                    };

                    pathNodeArray[pathNode.index] = pathNode;
                }
            }

            return pathNodeArray;
        }

        private static int CalculateDistanceCost(int2 aPos, int2 bPos)
        {
            int xDistance = Mathf.Abs(aPos.x - bPos.x);
            int yDistance = Mathf.Abs(aPos.y - bPos.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private static int CalculateIndex(int x, int y, int gridHeight)
        {
            var val = y + gridHeight * x;
            return val;
        }

        [BurstCompile]
        private struct FindPathJob : IJob
        {
            public int2 gridSize;

            public NativeArray<PathNode> pathNodeArray;

            public int2 startPos, endPos;

            public NativeList<int2> calculatedPathList;

            public void Execute()
            {
                NativeArray<int2> neighbourOffsetArray = GetNeighBorOffsetsArray();

                int endNodeIndex = CalculateIndex(endPos.x, endPos.y, gridSize.y);

                PathNode startNode = pathNodeArray[CalculateIndex(startPos.x, startPos.y, gridSize.y)];
                startNode.gCost = 0;
                startNode.CalculateFCost();
                pathNodeArray[startNode.index] = startNode;

                NativeList<int> openList = new NativeList<int>(Allocator.Temp);
                NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

                openList.Add(startNode.index);

                while (openList.Length > 0)
                {
                    int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);

                    PathNode currentNode = pathNodeArray[currentNodeIndex];

                    if (currentNodeIndex == endNodeIndex)
                    {
                        break;
                    }

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
                        int2 neighborOffset = neighbourOffsetArray[i];

                        int2 neighbourPosition = new int2(currentNode.x + neighborOffset.x,
                            currentNode.y + neighborOffset.y);

                        if (!IsPositionInsideGrid(neighbourPosition, gridSize)) continue;

                        int neighborNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.y);

                        if (closedList.Contains(neighborNodeIndex)) continue;

                        PathNode neighborNode = pathNodeArray[neighborNodeIndex];

                        if (!neighborNode.isWalkable) continue;

                        int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                        int tentativeGCost = currentNode.gCost +
                                             CalculateDistanceCost(currentNodePosition, neighbourPosition);

                        if (tentativeGCost < neighborNode.gCost)
                        {
                            neighborNode.cameFromNodeIndex = currentNodeIndex;

                            neighborNode.gCost = tentativeGCost;

                            neighborNode.CalculateFCost();

                            pathNodeArray[neighborNodeIndex] = neighborNode;

                            if (!openList.Contains(neighborNode.index))
                            {
                                openList.Add(neighborNode.index);
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
                    calculatedPathList.Add(new int2(endNode.x, endNode.y));

                    PathNode currentNode = endNode;

                    while (currentNode.cameFromNodeIndex != -1)
                    {
                        PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];

                        calculatedPathList.Add(new int2(cameFromNode.x, cameFromNode.y));

                        currentNode = cameFromNode;
                    }
                }
            }

            private NativeArray<int2> GetNeighBorOffsetsArray()
            {
                NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);

                neighbourOffsetArray[0] = new int2(-1, 0); // Left
                neighbourOffsetArray[1] = new int2(+1, 0); // Right
                neighbourOffsetArray[2] = new int2(0, +1); // Up
                neighbourOffsetArray[3] = new int2(0, -1); // Down
                neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
                neighbourOffsetArray[5] = new int2(-1, +1); // Left Up
                neighbourOffsetArray[6] = new int2(+1, -1); // Right Down
                neighbourOffsetArray[7] = new int2(+1, +1); // Right Up

                return neighbourOffsetArray;
            }

            private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
            {
                PathNode lowestCostPathNode = pathNodeArray[openList[0]];

                for (int i = 1; i < openList.Length; i++)
                {
                    PathNode testPathNode = pathNodeArray[openList[i]];

                    if (testPathNode.fCost < lowestCostPathNode.fCost)
                    {
                        lowestCostPathNode = testPathNode;
                    }
                }

                return lowestCostPathNode.index;
            }

            private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
            {
                return gridPosition.x >= 0 && gridPosition.y >= 0 && gridPosition.x < gridSize.x &&
                       gridPosition.y < gridSize.y;
            }
        }
    }
}