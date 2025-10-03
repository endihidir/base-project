using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityBase.Extensions;
using UnityBase.PathFinding;
using UnityBase.Pool;
using UnityEngine;
using Object = System.Object;

namespace UnityBase.GridSystem
{
    public class UIGrid<T> : IUIGrid<T> where T : struct, IGridNodeData
    {
        #region VARIABLES

        private readonly Camera _cam;
        protected int _width, _height;
        
        protected float _screenSidePaddingRatio;
        protected float _cellSpacingRatio;

        protected T[,] _gridArray;
        protected Vector3 _originOffset;
        
        protected float _cellSize;
        protected bool _drawGizmos;
        protected Color GizmosColor;

        #endregion

        #region PROPERTIES

        public int Width => _width;
        public int Height => _height;
        public bool DrawGizmos => _drawGizmos;
        
        public T[,] GridArray => _gridArray;
        public float CellSize => _cellSize;
        
        private static readonly Dictionary<Direction2D, Vector2Int> _baseDirections = new()
        {
            { Direction2D.Right,     new Vector2Int(1, 0) },
            { Direction2D.Left,      new Vector2Int(-1, 0) },
            { Direction2D.Up,        new Vector2Int(0, -1) },
            { Direction2D.Down,      new Vector2Int(0, 1) },
            { Direction2D.RightUp,   new Vector2Int(1, -1) },
            { Direction2D.LeftUp,    new Vector2Int(-1, -1) },
            { Direction2D.RightDown, new Vector2Int(1, 1) },
            { Direction2D.LeftDown,  new Vector2Int(-1, 1) }
        };

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
            var cellCount = Width * Height;
            
            for (int i = 0; i < cellCount; i++)
            {
                var pos = IndexToGridPosition(i);
                
                var data = new T
                {
                    GridPos = pos,
                    IsWalkable = true,
                    GCost = int.MaxValue,
                    HCost = 0,
                    FCost = 0,
                    CameFromNodeIndex = -1
                };

                _gridArray[pos.x, pos.y] = data;
            }
        }
        
        public void Update(int width, int height, float screenSidePaddingRatio, float cellSpacingRatio, Vector3 originOffset, bool drawGizmos, Color gizmosColor)
        {
            var widthChanged = width != Width;
            var heightChanged = height != Height;

            if (widthChanged || heightChanged)
            {
                PreserveOldData(width, height);
            }

            _screenSidePaddingRatio = screenSidePaddingRatio;
            _cellSpacingRatio = cellSpacingRatio;
            _originOffset = originOffset;
            _drawGizmos = drawGizmos;
            GizmosColor = gizmosColor;

            CalculateCellSize();
        }
        
        private void PreserveOldData(int newWidth, int newHeight)
        {
            var oldGrid = _gridArray;
            var oldWidth = Width;
            var oldHeight = Height;

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
        
        public virtual Vector3 GridToWorld(Vector3Int pos)
        {
            if (!IsInRange(pos)) return Vector3.zero;

            var screenWidth = GetScreenWidth();
            var screenHeight = GetScreenHeight();

            var gridOffsetX = CalculateGridOffset(screenWidth);
            var gridOffsetY = screenHeight * (_cellSpacingRatio / 100f);

            var totalGridWidth = (Width * _cellSize) + ((Width - 1) * gridOffsetX);
            var leftStartX = GetLeftX() + ((screenWidth - totalGridWidth) * 0.5f);

            var x = leftStartX + (pos.x * (_cellSize + gridOffsetX)) + (_cellSize * 0.5f);
            var y = GetTopY()   - (pos.y * (_cellSize + gridOffsetY)) - (_cellSize * 0.5f);

            return new Vector3(x, y, 0f);
        }
        
        public virtual Vector3Int WorldToGrid(Vector3 worldPos, bool clamp = true)
        {
            var position = new Vector3Int(-1, -1);
            
            var screenWidth = GetScreenWidth();
            var screenHeight = GetScreenHeight();

            var gridOffsetX = CalculateGridOffset(screenWidth);
            var gridOffsetY = screenHeight * (_cellSpacingRatio / 100f);

            var totalGridWidth = (Width * _cellSize) + ((Width - 1) * gridOffsetX);
            var leftStartX = GetLeftX() + ((screenWidth - totalGridWidth) * 0.5f);
            
            var absXFromGrid = worldPos.x - leftStartX;
            var absYFromTop  = GetTopY() - worldPos.y;

            var dividerX = _cellSize + gridOffsetX;
            var dividerY = _cellSize + gridOffsetY;
            
            if ((absXFromGrid % dividerX) > _cellSize || (absYFromTop % dividerY) > _cellSize)
            {
                return new Vector3Int(-1, -1);
            }

            var gx = Mathf.FloorToInt(absXFromGrid / dividerX);
            var gy = Mathf.FloorToInt(absYFromTop / dividerY);

            if (clamp)
            {
                gx = Mathf.Clamp(gx, 0, Width - 1);
                gy = Mathf.Clamp(gy, 0, Height - 1);
            }

            position.x = gx;
            position.y = gy;
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
            
            var pos = WorldToGrid(worldPosition, false);
            
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

        public bool TryGetNeighbor(T currentObject, Direction2D direction2D, out T neighbour)
        {
            neighbour = default;
            
            if (TryFindPositionOf(currentObject, out var pos))
            {
                return TryGetNeighbor(pos, direction2D, out neighbour);
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
            
            foreach (Direction2D direction in Enum.GetValues(typeof(Direction2D)))
            {
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

            if (!TryFindPositionOf(currentObject, out var pos)) return false;

            foreach (Direction2D direction in Enum.GetValues(typeof(Direction2D)))
            {
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
        
        public virtual bool TryGetNeighbor(Vector3Int pos, Direction2D direction2D, out T neighbour)
        {
            neighbour = default;
            
            if (direction2D == Direction2D.None)
            {
                if (IsInRange(pos))
                {
                    neighbour = _gridArray[pos.x, pos.y];
                    return !EqualityComparer<T>.Default.Equals(neighbour, default);
                }
                return false;
            }
            
            if (!_baseDirections.TryGetValue(direction2D, out var offset))
                return false;
            
            var newPos = new Vector3Int(pos.x + offset.x, pos.y + offset.y, 0);
            
            if (IsInRange(newPos))
            {
                neighbour = _gridArray[newPos.x, newPos.y];
                return !EqualityComparer<T>.Default.Equals(neighbour, default);
            }

            return false;
        }

        public bool TryFindPositionOf(T obj, out Vector3Int pos)
        {
            var cellCount = Width * Height;
            
            for (int i = 0; i < cellCount; i++)
            {
                var posInt = IndexToGridPosition(i);
                
                if (_gridArray[posInt.x, posInt.y].Equals(obj))
                {
                    pos = new Vector3Int(posInt.x, posInt.y, 0);
                    return true;
                }
                
            }

            pos = new Vector3Int(-1, -1, -1);
            return false;
        }

        public int GridPositionToIndex(Vector3Int pos) => (pos.y * Width) + pos.x;

        public Vector3Int IndexToGridPosition(int index)
        {
            var x = index % Width;
            var y = Mathf.FloorToInt(index / (float)Width);
            return new Vector3Int(x, y);
        }
        
        public virtual void DrawGrid()
        {
            if(!DrawGizmos) return;
    
            Gizmos.color = GizmosColor;
            
            var cellCount = Width * Height;

            for (int i = 0; i < cellCount; i++)
            {
                var pos = IndexToGridPosition(i);
                var center = GridToWorld(pos);
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
        
        public void DebugDrawPath(List<Vector3Int> path, float duration, Color color)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var start = GridToWorld(path[i]);
                var end = GridToWorld(path[i + 1]);
                Debug.DrawLine(start, end, color, duration);
            }
        }

        public void DebugDrawPath(NativeList<Vector3Int> path, float duration, Color color)
        {
            for (int i = 0; i < path.Length - 1; i++)
            {
                var start = GridToWorld(path[i]);
                var end = GridToWorld(path[i + 1]);
                Debug.DrawLine(start, end, color, duration);
            }
        }
        
        public virtual void RebuildMeshVisual(Mesh mesh)
        {
            var cellCount = Width * Height;
            
            MeshExtensions.CreateEmptyMeshArrays(cellCount, out var vertices, out var uvs, out var triangles);
            
            for (int i = 0; i < cellCount; i++)
            {
                var pos = IndexToGridPosition(i);
                var node = GetGridObject(pos);
                if (node.Equals(default(GridNode)) || node.IsWalkable) continue;

                var worldPos = GridToWorld(pos);
                var size = new Vector3(CellSize, CellSize, 0f);
                var index = GridPositionToIndex(pos);
                MeshExtensions.AddToMeshArrays(vertices, uvs, triangles, index, worldPos, 0f, size, Vector2.zero, Vector2.one);
                
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
        }
        
        public virtual List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos, bool allowDiagonal = false)
        {
            var cellCount = Width * Height;
            var pathNodeArray = new T[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                var pos = IndexToGridPosition(i);
                pathNodeArray[GridPositionToIndex(pos)] = GetGridObject(pos);
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
        
        public virtual NativeList<Vector3Int> FindPathWithJobs(Vector3Int startPos, Vector3Int endPos, bool allowDiagonal = false)
        {
            var cellCount = Width * Height;
            var pathNodeArray = new NativeArray<T>(cellCount, Allocator.TempJob);
            var result = new NativeList<Vector3Int>(Allocator.TempJob);

            for (int i = 0; i < cellCount; i++)
            {
                var pos = IndexToGridPosition(i);
                pathNodeArray[GridPositionToIndex(pos)] = GetGridObject(pos);
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
        
        public bool IsInRange(Vector3Int pos) => pos.x >= 0 && pos.y >= 0 && pos.x < Width && pos.y < Height;
        
        protected float GetScreenWidth() => Mathf.Abs(GetLeftX() - GetRightX());
        protected float GetScreenHeight() => Mathf.Abs(GetTopY() - GetBottomY());
        
        protected Vector3 GetOriginPos(Vector3 origin = default) => _cam.ViewportToWorldPoint(origin.With(z: _cam.nearClipPlane)) + new Vector3(_originOffset.x, -_originOffset.y, 0f);

        protected float GetRightX() => GetOriginPos(Vector3.right).x;
        protected float GetTopY() => GetOriginPos(Vector3.up).y;
        protected float GetLeftX() => GetOriginPos().x;
        protected float GetBottomY() => GetOriginPos().y;
        
        protected float CalculateBorderOffset(float screenWidth) => screenWidth * (_screenSidePaddingRatio / 100f);
        protected float CalculateGridOffset(float screenWidth) => screenWidth * (_cellSpacingRatio / 100f);
    }
}