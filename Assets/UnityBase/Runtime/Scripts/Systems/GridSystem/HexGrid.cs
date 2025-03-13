using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace UnityBase.GridSystem
{
    [Serializable]
    public sealed class HexGrid<T> where T : new()
    {
        public static readonly float2 DefaultOffsetMultiplier = new(0.82f, 0.708f);

        private IHexGridEntity _hexGridEntity;

        private T[,] _gridArray;
        public int Width => _hexGridEntity.Width;
        public int Height => _hexGridEntity.Height;
        public bool IsGridInitialized { get; set; }
        
        public HexGrid(IHexGridEntity hexGridEntity)
        {
            _hexGridEntity = hexGridEntity;
            
            _gridArray = new T[Width, Height];
        }

        ~HexGrid()
        {
            _hexGridEntity = default;

            IsGridInitialized = false;
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

            IsGridInitialized = true;
        }

        public float3 GetWorldPosition(int x, int y, int z)
        {
            var horizontalSize = _hexGridEntity.NodeSize.x;
            var verticalSize = _hexGridEntity.NodeSize.y;

            var offsetMultiplier = _hexGridEntity.OffsetMultiplier;

            if (_hexGridEntity.UseVerticalShape)
            {
                var verticalIndex = GetVerticalIndex(y, z);
                
                var horizontalOffset = (verticalIndex % 2) == 1 ? horizontalSize * 0.5f : 0f;

                var xPos = (x * horizontalSize + horizontalOffset) * offsetMultiplier.x;
                
                var yPos = (y * verticalSize * offsetMultiplier.y);

                var zPos = (z * verticalSize * offsetMultiplier.y);

                return new float3(xPos, yPos, zPos) + _hexGridEntity.OriginPos;
            }
            else
            {
                var verticalOffset = (x % 2) == 1 ? verticalSize * 0.5f : 0f;

                var xPos = (x * horizontalSize * offsetMultiplier.y);
                
                var yPos = (y * verticalSize + verticalOffset) * offsetMultiplier.x;

                var zPos = (z * verticalSize + verticalOffset) * offsetMultiplier.x;

                return new float3(xPos, yPos, zPos) + _hexGridEntity.OriginPos;
            }
        }

        public T GetGridObject(float3 worldPos)
        {
            Get(worldPos, out var x,out var y, out var z);

            var verticalIndex = GetVerticalIndex(y, z);

            var gridObj = GetGridObject(x, verticalIndex);

            return gridObj;
        }

        public void SetGridObject(float3 worldPos, T value)
        {
            Get(worldPos, out var x,out var y, out var z);
            
            var verticalIndex = GetVerticalIndex(y, z);
            
            if (!IsInRange(x, verticalIndex)) return;

            GetGridArray()[x, verticalIndex] = value;
        }

        public T GetGridObject(int x, int y)
        {
            if (!IsInRange(x, y)) return default;

            return GetGridArray()[x, y];
        }

        public bool IsInRange(int x, int z)
        {
            var isInRange = x >= 0 && z >= 0 && x < _hexGridEntity.Width && z < _hexGridEntity.Height;

            return isInRange;
        }

        private void Get(float3 worldPos, out int x, out int y, out int z)
        {
            var absolutePos = worldPos - _hexGridEntity.OriginPos;

            var widthDivider = _hexGridEntity.NodeSize.x;
            var heightDivider = _hexGridEntity.NodeSize.y;

            var offsetMultiplier = _hexGridEntity.OffsetMultiplier;

            var roughX = Mathf.RoundToInt(absolutePos.x / widthDivider / (_hexGridEntity.UseVerticalShape ? offsetMultiplier.x : offsetMultiplier.y));
            var roughY = Mathf.RoundToInt(absolutePos.y / heightDivider / (_hexGridEntity.UseVerticalShape ? offsetMultiplier.y : offsetMultiplier.x));
            var roughZ = Mathf.RoundToInt(absolutePos.z / heightDivider / (_hexGridEntity.UseVerticalShape ? offsetMultiplier.y : offsetMultiplier.x));
            
            var roughPos = new int3(roughX, roughY, roughZ);

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

        private List<int3> GetNeighbours(int3 roughPos)
        {
            if (_hexGridEntity.UseVerticalShape)
            {
                var vPos = GetVerticalIndex(roughPos.y, roughPos.z);
                
                var oddRow = vPos % 2 == 1;

                return new List<int3>()
                {
                    roughPos + new int3(0, 0, 1), // Top
                    roughPos + new int3(0, 0, -1),// Bottom
                    
                    roughPos + new int3(1, 0, 0), // right
                    roughPos + new int3(-1, 0, 0), // Left

                    roughPos + new int3(oddRow ? 1 : -1, 0, 1), // Top right or Top Left
                    roughPos + new int3(oddRow ? 1 : -1, 0, -1), // Bottom right or Bottom Left
                };
            }
            else
            {
                var oddColumn = roughPos.x % 2 == 1;

                return new List<int3>()
                {
                    roughPos + new int3(0, 0, 1), // Top
                    roughPos + new int3(0, 0, -1), // Bottom
                    
                    roughPos + new int3(1, 0, 0), // right
                    roughPos + new int3(-1, 0, 0), // left

                    roughPos + new int3(1, 0, oddColumn ? 1 : -1), // right Top or right Bottom
                    roughPos + new int3(-1, 0, oddColumn ? 1 : -1), // left Top or left Bottom
                };
            }
        }

        public T[] GetObjectNeighbours(float3 worldPos)
        {
            Get(worldPos, out var x,out var y, out var z);

            var neighbourIndexList = GetNeighbours(new int3(x, y, z));
            
            var neighbours = new List<T>();

            foreach (var neighbourIndex in neighbourIndexList)
            {
                var neighbour = GetGridObject(neighbourIndex.x, neighbourIndex.z);
                
                if (neighbour != null)
                {
                    neighbours.Add(neighbour);
                }
            }

            return neighbours.ToArray();
        }

        private T[,] GetGridArray() => _gridArray;

        public int CalculateIndex(int x, int y, int gridHeight)
        {
            var val = (x * gridHeight) + y;
            return val;
        }
        
        private int GetVerticalIndex(int y, int z)
        {
            var vIndex = _hexGridEntity.Is2DGrid ? y : z;
            return vIndex;
        }
    }
}