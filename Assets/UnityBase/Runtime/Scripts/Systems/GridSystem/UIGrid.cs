using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityBase.Extensions;
using UnityBase.PathFinding;
using UnityEngine;

namespace UnityBase.GridSystem
{
    public interface IUIGrid<T> : IGrid<T> where T : struct
    {
        Vector3 GetWorldPosition(Vector3Int pos);
        Vector3Int WorldToGrid(Vector3 worldPos);
        bool TryGetGridObjectFromMousePosition(out T obj);
        bool TryFindPositionOf(T obj, out Vector3Int pos);
        bool TryGetNeighbor(T currentObject, Direction direction, out T neighbour);
        bool TryGetNeighbor(Vector3Int pos, Direction direction, out T neighbour);
        bool TryGetNeighbors(T currentObject, out T[] neighbours);
        bool TryGetNeighborsNonAlloc(T currentObject, Span<T> resultBuffer, out int count);

        public void Update(int width, int height, float screenSidePaddingRatio, float cellSpacingRatio, Vector3 originOffset, bool drawGizmos, Color gizmosColor);
    }
    
    public class UIGrid<T> : IUIGrid<T> where T : struct, IGridNodeData
    {
        #region VARIABLES

        private readonly Camera _cam;
        private int _width, _height;
        
        private float _screenSidePaddingRatio;
        private float _cellSpacingRatio;

        private T[,] _gridArray;
        private Vector3 _originOffset;
        
        private float _cellSize;
        private bool _drawGizmos;
        protected Color GizmosColor;

        #endregion

        #region PROPERTIES

        public int Width => _width;
        public int Height => _height;
        public bool DrawGizmos => _drawGizmos;

        public T[,] GridArray => _gridArray;
        public float CellSize => _cellSize;

        #endregion

        public UIGrid(Camera cam, int width, int height, float screenSidePaddingRatio, float cellSpacingRatio, Vector3 originOffset, bool drawGizmos, Color gizmosColor)
        {
            _cam = cam;
            _width = width;
            _height = height;
            _screenSidePaddingRatio = screenSidePaddingRatio;
            _cellSpacingRatio = cellSpacingRatio;
            _originOffset = originOffset;
            _drawGizmos = drawGizmos;
            GizmosColor = gizmosColor;
            _gridArray = new T[Width, Height];

            Initialize();
            CalculateCellSize();
        }
        
        private void Initialize()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var pos = new Vector3Int(x, y);
                    
                    var data = new T
                    {
                        GridPos = pos,
                        IsWalkable = true,
                        GCost = int.MaxValue,
                        HCost = 0,
                        FCost = 0,
                        CameFromNodeIndex = -1
                    };

                    _gridArray[x, y] = data;
                }
            }
        }
        
        public void Update(int width, int height, float screenSidePaddingRatio, float cellSpacingRatio, Vector3 originOffset, bool drawGizmos, Color gizmosColor)
        {
            var widthChanged = width != _width;
            var heightChanged = height != _height;

            if (widthChanged || heightChanged)
            {
                ResizePreserveOldData(width, height);
            }

            _screenSidePaddingRatio = screenSidePaddingRatio;
            _cellSpacingRatio = cellSpacingRatio;
            _originOffset = originOffset;
            _drawGizmos = drawGizmos;
            GizmosColor = gizmosColor;

            CalculateCellSize();
        }
        
        private void ResizePreserveOldData(int newWidth, int newHeight)
        {
            var oldGrid = _gridArray;
            var oldWidth = _width;
            var oldHeight = _height;

            var newGrid = new T[newWidth, newHeight];

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    var pos = new Vector3Int(x, y);

                    if (x < oldWidth && y < oldHeight)
                    {
                        newGrid[x, y] = oldGrid[x, y];
                    }
                    else
                    {
                        newGrid[x, y] = new T
                        {
                            GridPos = pos,
                            IsWalkable = true,
                            GCost = int.MaxValue,
                            HCost = 0,
                            FCost = 0,
                            CameFromNodeIndex = -1
                        };
                    }
                }
            }

            _gridArray = newGrid;
            _width = newWidth;
            _height = newHeight;
        }
        
        private void CalculateCellSize()
        {
            var screenWidth = GetScreenWidth();
            var screenHeight = GetScreenHeight();

            var borderOffsetW = screenWidth * (_screenSidePaddingRatio / 100f);
            var gridOffsetW = screenWidth * (_cellSpacingRatio / 100f);
            var gridOffsetH = screenHeight * (_cellSpacingRatio / 100f);

            var cellSizeW = (screenWidth - borderOffsetW - (gridOffsetW * (Width - 1))) / Width;
            var totalGridHeight = (cellSizeW * Height) + (gridOffsetH * (Height - 1));
            
            var maxWorldHeight = screenHeight * 0.9f;

            if (totalGridHeight > maxWorldHeight)
            {
                _cellSize = (maxWorldHeight - (gridOffsetH * (Height - 1))) / Height;
            }
            else
            {
                _cellSize = cellSizeW;
            }
        }
        
        public Vector3 GetWorldPosition(Vector3Int pos)
        {
            if (!IsInRange(pos)) return Vector3.zero;

            var borderOffset = GetScreenWidth() * (_screenSidePaddingRatio / 100f);
            var gridOffset = GetScreenWidth() * (_cellSpacingRatio / 100f);
            
            var totalGridWidth = (Width * _cellSize) + ((Width - 1) * gridOffset);
            
            var xOffset = (GetScreenWidth() - totalGridWidth) / 2 + pos.x * (_cellSize + gridOffset);
            var yOffset = (borderOffset / 2) + pos.y * (_cellSize + gridOffset);
    
            var position = new Vector3(GetLeftX() + xOffset + (_cellSize / 2), GetTopY() - yOffset - (_cellSize / 2), 0);

            return position;
        }

        public T GetGridObject(Vector3Int pos)
        {
            if (!IsInRange(pos)) return default;

            return _gridArray[pos.x, pos.y];
        }
        
        public bool TryGetGridObjectFromMousePosition(out T obj)
        {
            var worldPosition = _cam.ScreenToWorldPoint(Input.mousePosition).With(z: _cam.nearClipPlane);
            var pos = WorldToGrid(worldPosition);
            if (!IsInRange(pos))
            {
                obj = default;
                return false;
            }

            obj = _gridArray[pos.x, pos.y];
            return true;
        }

        public T GetGridObject(Vector3 worldPos)
        {
            var pos = WorldToGrid(worldPos);
            if (!IsInRange(pos)) return default;
            return _gridArray[pos.x, pos.y];
        }

        public void SetGridObject(Vector3Int pos, T value)
        {
            if (!IsInRange(pos)) return;

            _gridArray[pos.x, pos.y] = value;
        }

        public bool TryGetNeighbor(T currentObject, Direction direction, out T neighbour)
        {
            neighbour = default;
            
            if (TryFindPositionOf(currentObject, out var pos))
            {
                return TryGetNeighbor(pos, direction, out neighbour);
            }
            
            return false;
        }

        public bool TryGetNeighbors(T currentObject, out T[] neighbours)
        {
            if (!TryFindPositionOf(currentObject, out var pos))
            {
                neighbours = Array.Empty<T>();
                return false;
            }

            var result = new List<T>();
            
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (direction is Direction.None or Direction.Forward or Direction.Backward)
                    continue;
                
                if (TryGetNeighbor(pos, direction, out var neighbour))
                {
                    result.Add(neighbour);
                }
            }

            neighbours = result.ToArray();
            return neighbours.Length > 0;
        }
        
        public bool TryGetNeighborsNonAlloc(T currentObject, Span<T> resultBuffer, out int count)
        {
            count = 0;

            if (!TryFindPositionOf(currentObject, out var pos))
                return false;

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (direction is Direction.None or Direction.Forward or Direction.Backward)
                    continue;

                if (TryGetNeighbor(pos, direction, out var neighbour))
                {
                    if (count >= resultBuffer.Length)
                    {
                        return true;
                    }

                    resultBuffer[count++] = neighbour;
                }
            }

            return count > 0;
        }

        public bool TryGetNeighbor(Vector3Int pos, Direction direction, out T neighbour)
        {
            neighbour = default;

            switch (direction)
            {
                case Direction.Right: pos.x++; break;
                case Direction.Left:  pos.x--; break;
                case Direction.Up:    pos.y--; break;
                case Direction.Down:  pos.y++; break;
            }

            if (IsInRange(pos))
            {
                neighbour = _gridArray[pos.x, pos.y];
                return !EqualityComparer<T>.Default.Equals(neighbour, default);
            }

            return false;
        }

        public bool TryFindPositionOf(T obj, out Vector3Int pos)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_gridArray[x, y].Equals(obj))
                    {
                        pos = new Vector3Int(x, y, 0);
                        return true;
                    }
                }
            }

            pos = new Vector3Int(-1, -1, -1);
            return false;
        }

        public bool IsInRange(Vector3Int pos) => pos.x >= 0 && pos.y >= 0 && pos.x < Width && pos.y < Height;

        public Vector3Int WorldToGrid(Vector3 worldPos)
        {
            var pos = new Vector3Int(-1, -1);
            
            var absoluteXPos = worldPos.x - GetOriginPos(Vector3.up).x;
            var absoluteYPos = GetOriginPos(Vector3.up).y - worldPos.y;

            var screenWidth = GetScreenWidth();
            var borderOffset = CalculateBorderOffset(screenWidth);
            var gridOffset = CalculateGridOffset(screenWidth);

            var xDivider = _cellSize + gridOffset;
            var yDivider = _cellSize + gridOffset;

            var xRaw = (absoluteXPos - (borderOffset / 2)) / xDivider;
            var yRaw = (absoluteYPos - (borderOffset / 2)) / yDivider;
            
            if (IsInPaddingArea(absoluteXPos, xDivider, borderOffset) || IsInPaddingArea(absoluteYPos, yDivider, borderOffset))
            {
                return pos;
            }
            
            pos.x = Mathf.FloorToInt(xRaw);
            pos.y = Mathf.FloorToInt(yRaw);

            return pos;
        }

        public int GridPositionToIndex(Vector3Int pos) => (pos.y * Width) + pos.x;

        public Vector3Int IndexToGridPosition(int index)
        {
            var x = index % Width;
            var y = Mathf.FloorToInt(index / (float)Width);
            return new Vector3Int(x, y);
        }
        
        public void DrawGrid()
        {
            if(!DrawGizmos) return;
    
            Gizmos.color = GizmosColor;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var center = GetWorldPosition(new Vector3Int(x, y));
                    var halfSize = _cellSize * 0.5f;
                    
                    var topLeft = new Vector3(center.x - halfSize, center.y + halfSize, center.z);
                    var topRight = new Vector3(center.x + halfSize, center.y + halfSize, center.z);
                    var bottomRight = new Vector3(center.x + halfSize, center.y - halfSize, center.z);
                    var bottomLeft = new Vector3(center.x - halfSize, center.y - halfSize, center.z);
                    
                    Gizmos.DrawLine(topLeft, topRight);    
                    Gizmos.DrawLine(topRight, bottomRight); 
                    Gizmos.DrawLine(bottomRight, bottomLeft);
                    Gizmos.DrawLine(bottomLeft, topLeft);
                }
            }
        }
        
        public void DebugDrawPath(List<Vector3Int> path, float duration, Color color)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var start = GetWorldPosition(path[i]);
                var end = GetWorldPosition(path[i + 1]);
                Debug.DrawLine(start, end, color, duration);
            }
        }

        public void DebugDrawPath(NativeList<Vector3Int> path, float duration, Color color)
        {
            for (int i = 0; i < path.Length - 1; i++)
            {
                var start = GetWorldPosition(path[i]);
                var end = GetWorldPosition(path[i + 1]);
                Debug.DrawLine(start, end, color, duration);
            }
        }
        
        public void RebuildMeshVisual(Mesh mesh)
        {
            var cellCount = _width * _height;
            MeshUtils.CreateEmptyMeshArrays(cellCount, out var vertices, out var uvs, out var triangles);
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var pos = new Vector3Int(x, y);
                    var node = GetGridObject(pos);
                    if (node.Equals(default(GridNode)) || node.IsWalkable) continue;

                    var worldPos = GetWorldPosition(pos);
                    var size = new Vector3(CellSize, CellSize, 0f);
                    var index = GridPositionToIndex(pos);
                    MeshUtils.AddToMeshArrays(vertices, uvs, triangles, index, worldPos, 0f, size, Vector2.zero, Vector2.one);
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
        }
        
        public List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos, bool allowDiagonal = false)
        {
            var totalSize = Width * Height;
            var pathNodeArray = new T[totalSize];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var pos = new Vector3Int(x, y);
                    pathNodeArray[GridPositionToIndex(pos)] = GetGridObject(pos);
                }
            }

            var pathFinder = new FindPath<T>
            {
                GridSize = new int3(Width, Height, 1),
                PathNodeArray = pathNodeArray,
                StartPos = startPos,
                EndPos = endPos,
                AllowDiagonalCornerCutting = allowDiagonal
            };

            return pathFinder.Execute();
        }
        
        public NativeList<Vector3Int> FindPathWithJobs(Vector3Int startPos, Vector3Int endPos, bool allowDiagonal = false)
        {
            var totalSize = Width * Height;
            var pathNodeArray = new NativeArray<T>(totalSize, Allocator.TempJob);
            var result = new NativeList<Vector3Int>(Allocator.TempJob);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var pos = new Vector3Int(x, y);
                    pathNodeArray[GridPositionToIndex(pos)] = GetGridObject(pos);
                }
            }

            var job = new FindPathJob<T>
            {
                GridSize = new int3(Width, Height, 1),
                PathNodeArray = pathNodeArray,
                StartPos = startPos,
                EndPos = endPos,
                CalculatedPathList = result,
                AllowDiagonalCornerCutting = allowDiagonal
            };

            job.Schedule().Complete();
            pathNodeArray.Dispose();
            return result;
        }
        
        private float GetScreenWidth() => Mathf.Abs(GetLeftX() - GetRightX());
        private float GetScreenHeight() => Mathf.Abs(GetTopY() - GetBottomY());
        
        private Vector3 GetOriginPos(Vector3 origin = default) => _cam.ViewportToWorldPoint(origin.With(z: _cam.nearClipPlane)) - new Vector3(_originOffset.x, _originOffset.y, 0f);

        private float GetRightX() => GetOriginPos(Vector3.right).x;
        private float GetTopY() => GetOriginPos(Vector3.up).y;
        private float GetLeftX() => GetOriginPos().x;
        private float GetBottomY() => GetOriginPos().y;
        
        private float CalculateBorderOffset(float screenWidth) => screenWidth * (_screenSidePaddingRatio / 100f);
        private float CalculateGridOffset(float screenWidth) => screenWidth * (_cellSpacingRatio / 100f);

        private bool IsInPaddingArea(float absolutePos, float divider, float borderOffset) => (absolutePos - (borderOffset / 2)) % divider > _cellSize;
    }
}