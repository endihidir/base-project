using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace UnityBase.GridSystem
{
    public interface IHexTileGrid : IGrid
    {
        bool UseVerticalShape { get; }
    }
    
    [Serializable]
    public sealed class HexTileGrid<T> : IHexTileGrid where T : struct, IPathNodeData
    {
        public static readonly float2 DefaultOffsetMultiplier = new(0.82f, 0.708f);

        private int _width, _height;
        private Vector3 _cellSize;
        private Vector3 _cellOffset, _gridOffset;
        private bool _useVerticalShape;
        
        private T[,] _gridArray;
        public int Width => _width;
        public int Height => _height;
        public Vector3 CellSize => _cellSize;
        public Vector3 CellOffset => _cellOffset;
        public Vector3 GridOffset => _gridOffset;
        public bool UseVerticalShape => _useVerticalShape;
        
        public HexTileGrid(int width, int height, Vector3 cellSize, Vector3 cellOffset, Vector3 gridOffset, bool useVerticalShape)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _cellOffset = cellOffset;
            _gridOffset = gridOffset;
            _useVerticalShape = useVerticalShape;
            _gridArray = new T[Width, Height];
        }

        public void FillArray(int x, int y, T gridNode)
        {
            _gridArray[x, y] = gridNode;
        }

        public void FillArray(T[] gridArray)
        {
            _gridArray = new T[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var index = CalculateIndex(x, y, Height);

                    _gridArray[x, y] = gridArray[index];
                }
            }
        }

        public Vector3 GetWorldPosition(int x, int y, int z)
        {
            var horizontalSize = CellSize.x;
            var verticalSize = CellSize.y;

            var offsetMultiplier = CellOffset;

            if (UseVerticalShape)
            {
                var horizontalOffset = (y % 2) == 1 ? horizontalSize * 0.5f : 0f;

                var xPos = (x * horizontalSize + horizontalOffset) * offsetMultiplier.x;
                
                var yPos = (y * verticalSize * offsetMultiplier.y);

                var zPos = (z * verticalSize * offsetMultiplier.y);

                return new Vector3(xPos, yPos, zPos) + GridOffset;
            }
            else
            {
                var verticalOffset = (x % 2) == 1 ? verticalSize * 0.5f : 0f;

                var xPos = (x * horizontalSize * offsetMultiplier.y);
                
                var yPos = (y * verticalSize + verticalOffset) * offsetMultiplier.x;

                var zPos = (z * verticalSize + verticalOffset) * offsetMultiplier.x;

                return new Vector3(xPos, yPos, zPos) + GridOffset;
            }
        }

        public T GetGridObject(Vector3 worldPos)
        {
            Get(worldPos, out var x,out var y, out var z);

            var gridObj = GetGridObject(x, y);

            return gridObj;
        }

        public void SetGridObject(Vector3 worldPos, T value)
        {
            Get(worldPos, out var x,out var y, out var z);
            
            if (!IsInRange(x, y)) return;

            GetGridArray()[x, y] = value;
        }

        public T GetGridObject(int x, int y)
        {
            if (!IsInRange(x, y)) return default;

            return GetGridArray()[x, y];
        }

        public bool IsInRange(int x, int z)
        {
            var isInRange = x >= 0 && z >= 0 && x < Width && z < Height;

            return isInRange;
        }

        private void Get(Vector3 worldPos, out int x, out int y, out int z)
        {
            var absolutePos = worldPos - GridOffset;

            var widthDivider = CellSize.x;
            var heightDivider = CellSize.y;

            var offsetMultiplier = CellOffset;

            var roughX = Mathf.RoundToInt(absolutePos.x / widthDivider / (UseVerticalShape ? offsetMultiplier.x : offsetMultiplier.y));
            var roughY = Mathf.RoundToInt(absolutePos.y / heightDivider / (UseVerticalShape ? offsetMultiplier.y : offsetMultiplier.x));
            var roughZ = Mathf.RoundToInt(absolutePos.z / heightDivider / (UseVerticalShape ? offsetMultiplier.y : offsetMultiplier.x));
            
            var roughPos = new Vector3Int(roughX, roughY, roughZ);

            var neighbourList = GetNeighbours(roughPos);

            var closest = roughPos;

            foreach (var neighbour in neighbourList)
            {
                var dist = Vector3.Distance(worldPos, GetWorldPosition(neighbour.x, neighbour.y, neighbour.z));
                var closestDist = Vector3.Distance(worldPos, GetWorldPosition(closest.x, closest.y, closest.z));

                if (dist < closestDist)
                {
                    closest = neighbour;
                }
            }

            x = closest.x;
            y = closest.y;
            z = closest.z;
        }

        private List<Vector3Int> GetNeighbours(Vector3Int roughPos)
        {
            if (UseVerticalShape)
            {
                var oddRow = roughPos.y % 2 == 1;

                return new List<Vector3Int>()
                {
                    roughPos + new Vector3Int(0, 0, 1), // Top
                    roughPos + new Vector3Int(0, 0, -1),// Bottom
                    
                    roughPos + new Vector3Int(1, 0, 0), // right
                    roughPos + new Vector3Int(-1, 0, 0), // Left

                    roughPos + new Vector3Int(oddRow ? 1 : -1, 0, 1), // Top right or Top Left
                    roughPos + new Vector3Int(oddRow ? 1 : -1, 0, -1), // Bottom right or Bottom Left
                };
            }

            var oddColumn = roughPos.x % 2 == 1;

            return new List<Vector3Int>()
            {
                roughPos + new Vector3Int(0, 0, 1), // Top
                roughPos + new Vector3Int(0, 0, -1), // Bottom
                    
                roughPos + new Vector3Int(1, 0, 0), // right
                roughPos + new Vector3Int(-1, 0, 0), // left

                roughPos + new Vector3Int(1, 0, oddColumn ? 1 : -1), // right Top or right Bottom
                roughPos + new Vector3Int(-1, 0, oddColumn ? 1 : -1), // left Top or left Bottom
            };
        }

        public T[] GetObjectNeighbours(Vector3 worldPos)
        {
            Get(worldPos, out var x,out var y, out var z);

            var neighbourIndexList = GetNeighbours(new Vector3Int(x, y, z));
            
            var neighbours = new List<T>();

            foreach (var neighbourIndex in neighbourIndexList)
            {
                var neighbour = GetGridObject(neighbourIndex.x, neighbourIndex.z);

                neighbours.Add(neighbour);
            }

            return neighbours.ToArray();
        }

        private T[,] GetGridArray() => _gridArray;

        public int CalculateIndex(int x, int y, int gridHeight)
        {
            var val = (x * gridHeight) + y;
            return val;
        }
    }
}