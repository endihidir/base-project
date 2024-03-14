using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace UnityBase.GridSystem
{
    [Serializable]
    public sealed class Grid<T> where T : new()
    {
        #region VARIABLES

        private IGridEntity _gridEntity;

        [ShowInInspector] private T[,] _gridArray;
        
        #endregion

        #region PROPERTIES
        
        [ShowInInspector] public int Width => _gridEntity?.Width ?? 0;
        [ShowInInspector] public int Height => _gridEntity?.Height ?? 0;
        [ShowInInspector] public float NodeSize => _gridEntity?.NodeSize ?? 1;
        [ShowInInspector] public float3 Padding => _gridEntity?.Padding ?? float3.zero;
        public T[,] GridArray => _gridArray;

        #endregion

        public Grid(IGridEntity gridEntity)
        {
            _gridEntity = gridEntity;
            
            _gridArray = new T[Width, Height];
        }

        ~Grid() => _gridEntity = null;

        public float3 GetWorldPosition(int x, int y, int z)
        {
            var xPos = x * (_gridEntity.NodeSize + _gridEntity.Padding.x);
            var yPos = y * (_gridEntity.NodeSize + _gridEntity.Padding.y);
            var zPos = z * (_gridEntity.NodeSize + _gridEntity.Padding.z);
            
            var worldPos = new float3(xPos , yPos, zPos) + _gridEntity.OriginPos;
            
            return worldPos;
        }

        public T GetGridObject(float3 worldPos)
        {
            Get(worldPos, out var x, out var y, out var z);
            
            var verticalIndex = GetVerticalIndex(y, z);

            var gridObj = GetGridObject(x, verticalIndex);
            
            return gridObj;
        }

        public T GetGridObject(int x, int y)
        {
            if (!IsInRange(x, y)) return default;
            
            return _gridArray[x, y];
        }
        
        public void SetGridObject(float3 worldPos, T value)
        {
            Get(worldPos, out var x,out var y, out var z);

            var verticalIndex = GetVerticalIndex(y, z);
            
            if(!IsInRange(x, verticalIndex)) return;
            
            _gridArray[x, verticalIndex] = value;
        }
        
        public void SetGridObject(int x, int y, T gridNode)
        {
            _gridArray[x, y] = gridNode;
        }

        public bool IsInRange(int x, int y)
        {
            bool isInRange = x >= 0 && y >= 0 && x < _gridEntity.Width && y < _gridEntity.Height;
            return isInRange;
        }

        public T GetNeighbour(int index, Direction direction)
        {
            ReverseCalculateIndex(index, out var x, out var z);

            switch (direction)
            {
                case Direction.Right:
                    x++;
                    break;
                case Direction.Left:
                    x--;
                    break;
                case Direction.Up:
                    z++;
                    break;
                case Direction.Down:
                    z--;
                    break;
            }

            var worldPos = GetWorldPosition(x, 0, z);
            
            return GetGridObject(worldPos);
        }

        public void Get(float3 worldPos, out int x, out int y, out int z)
        {
            var absolutePos = worldPos - _gridEntity.OriginPos;

            var xDivider = _gridEntity.NodeSize + _gridEntity.Padding.x;
            var yDivider = _gridEntity.NodeSize + _gridEntity.Padding.y;
            var zDivider = _gridEntity.NodeSize + _gridEntity.Padding.z;
            
            x = Mathf.FloorToInt(absolutePos.x / xDivider);
            y = Mathf.FloorToInt(absolutePos.y / yDivider);
            z = Mathf.FloorToInt(absolutePos.z / zDivider);
        }

        public int CalculateIndex(int x, int z)
        {
            var val = (z * _gridEntity.Width) + x;
            return val;
        }
        
        public void ReverseCalculateIndex(int index, out int x, out int z)
        {
            x = index % _gridEntity.Width;
            z = Mathf.FloorToInt(index / (float)_gridEntity.Width);
        }
        
        private int GetVerticalIndex(int y, int z)
        {
            var vIndex = _gridEntity.Is2DGrid ? y : z;
            return vIndex;
        }
    }
}