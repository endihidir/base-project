using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityBase.PathFinding;
using UnityEngine;

namespace UnityBase.GridSystem
{
    public partial class WorldGrid<T> : IWorldGrid<T> where T : struct, IGridNodeData
    {
        private int _gridWidth;
        private int _gridHeight;
        private int _gridDepth;
        private bool _drawGizmos;
        private Vector3 _gridCellSize;
        private Vector3 _gridOffset;
        private Vector3 _cellOffset;
        protected Color GizmosColor;
        
        private readonly Transform _transform;
        
        private readonly Dictionary<Vector3Int, List<T>> _itemList = new();
        
        private static readonly Dictionary<Direction2D, Vector3Int> _baseDirections = new()
        {
            { Direction2D.Self, new Vector3Int(0, 0, 0) },
            { Direction2D.Right, new Vector3Int(1, 0, 0) },
            { Direction2D.Left, new Vector3Int(-1, 0, 0) },
            { Direction2D.Up, new Vector3Int(0, 1, 0) },
            { Direction2D.Down, new Vector3Int(0, -1, 0) },
            { Direction2D.RightDown, new Vector3Int(1, -1, 0) },
            { Direction2D.LeftDown,  new Vector3Int(-1, -1, 0) },
            { Direction2D.LeftUp, new Vector3Int(-1, 1, 0) },
            { Direction2D.RightUp,  new Vector3Int(1, 1, 0) },
        };
        
        public int Width => _gridWidth;
        public int Height => _gridHeight;
        public int Depth => _gridDepth;
        public Vector3 CellSize => _gridCellSize;
        public Vector3 CellOffset => _cellOffset;
        public Vector3 GridOffset => _gridOffset;
        public bool DrawGizmos => _drawGizmos;
        public Transform Transform => _transform;
        
        public WorldGrid(Transform coreTransform, int width, int height, int depth, Vector3 cellSize, Vector3 offset, Vector3 cellOffset, bool drawGizmos, Color gizmosColor)
        {
            _transform = coreTransform;
            _gridWidth = width;
            _gridHeight = height;
            _gridDepth = depth;
            _gridCellSize = cellSize;
            _gridOffset = offset;
            _drawGizmos = drawGizmos;
            GizmosColor = gizmosColor;
            _cellOffset = cellOffset;
            Initialize();
        }
        
        private void Initialize()
        {
            _itemList.Clear();
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        
                        var data = new T
                        {
                            GridPos = pos,
                            IsWalkable = true,
                            GCost = int.MaxValue,
                            HCost = 0,
                            FCost = 0,
                            CameFromNodeIndex = -1
                        };
                        
                        SetGridObject(pos, data);
                    }
                }
            }
        }

        public void Update(int width, int height, int depth, Vector3 cellSize, Vector3 offset, Vector3 cellOffset,bool drawGizmos, Color gizmosColor)
        {
            var widthChanged = width != _gridWidth;
            var heightChanged = height != _gridHeight;
            var depthChanged = depth != _gridDepth;
            
            _gridWidth = width;
            _gridHeight = height;
            _gridDepth = depth;
            _gridCellSize = cellSize;
            _gridOffset = offset;
            _drawGizmos = drawGizmos;
            GizmosColor = gizmosColor;
            _cellOffset = cellOffset;
            
            if ((widthChanged || heightChanged || depthChanged))
            {
                PreserveOldDataAndExpand(_gridWidth, _gridHeight, _gridDepth);
            }
        }
        
        private void PreserveOldDataAndExpand(int newWidth, int newHeight, int newDepth)
        {
            var oldItemList = new Dictionary<Vector3Int, List<T>>(_itemList);
            _itemList.Clear();

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int z = 0; z < newDepth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);

                        if (oldItemList.TryGetValue(pos, out var existingList))
                        {
                            _itemList[pos] = existingList;
                        }
                        else
                        {
                            var newItem = new T
                            {
                                GridPos = pos,
                                IsWalkable = true,
                                GCost = int.MaxValue,
                                HCost = 0,
                                FCost = 0,
                                CameFromNodeIndex = -1
                            };
                            
                            _itemList[pos] = new List<T> { newItem };
                        }
                    }
                }
            }
        }

        public int GetNeighborsNonAlloc(Vector3Int gridPos, int radius, T[] resultBuffer, bool includeSelf = false, bool includeDepth = false, bool includeDiagonal = true)
        {
            var yBegin = gridPos.y - radius;
            var yEnd = gridPos.y + radius;

            var xBegin = gridPos.x - radius;
            var xEnd = gridPos.x + radius;

            var zBegin = gridPos.z - radius;
            var zEnd = gridPos.z + radius;

            if (includeDepth)
            {
                zBegin = Mathf.Max(0, gridPos.z - radius);
                zEnd = Mathf.Min(_gridDepth - 1, gridPos.z + radius);
            }
            else
            {
                zBegin = zEnd = gridPos.z;
            }

            var count = 0;

            for (int y = yBegin; y <= yEnd; y++)
            {
                for (int x = xBegin; x <= xEnd; x++)
                {
                    for (int z = zBegin; z <= zEnd; z++)
                    {
                        var pos = new Vector3Int(x, y, z);

                        //don't include self if not specified
                        if (pos == gridPos && !includeSelf) continue;
                        
                        if (!IsInRange(pos)) continue;

                        //skip if no items exists
                        if (!_itemList.TryGetValue(pos, out var items)) continue;
                        
                        if (!includeDiagonal)
                        {
                            var diff = gridPos - pos;
                            var axisChanged = 0;

                            if (diff.x != 0) axisChanged++;
                            if (diff.y != 0) axisChanged++;
                            if (includeDepth && diff.z != 0) axisChanged++;

                            if (axisChanged > 1) continue;
                        }

                        //add found items to buffer
                        foreach (var item in items)
                        {
                            resultBuffer[count] = item;
                            count++;
                            if (count >= resultBuffer.Length)
                            {
                                Debug.LogError("TryGetNeighbors resultBuffer Capacity Full");
                                return count;
                            }
                        }
                    }
                }
            }

            return count;
        }
        
        public virtual bool TryGetNeighbor(Vector3Int pos, Direction2D direction2D, out T neighbor, DepthDirection depthDirection = default, bool useWorldDirection = true)
        {
            neighbor = default;
            
            if (_baseDirections.TryGetValue(direction2D, out var baseDir))
            {
                Vector3Int neighborGridPos;
                
                if (useWorldDirection)
                {
                    if (depthDirection != DepthDirection.None)
                    {
                        baseDir.z = depthDirection == DepthDirection.Forward ? -1 : 1;
                    }
                    
                    var worldOffset = Vector3.Scale(baseDir, CellSize + CellOffset);
                    var neighborWorldPos = GridToWorld(pos) + worldOffset;
                    neighborGridPos = WorldToGrid(neighborWorldPos);
                }
                else
                {
                    if (depthDirection != DepthDirection.None)
                    {
                        baseDir.z = depthDirection == DepthDirection.Forward ? 1 : -1;
                    }
                    
                    neighborGridPos = pos + baseDir;
                }

                if (IsInRange(neighborGridPos))
                {
                    var baseItem = GetGridObject(neighborGridPos);
                    
                    if (!baseItem.Equals(default(T)))
                    {
                        neighbor = baseItem;
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        public bool TryGetNodeFromScreenRay(Ray ray, int activeDepth, out Vector3Int gridPos, bool clamp)
        {
            gridPos = default;
            
            if (Depth <= activeDepth) return false;
            
            var plane = new Plane(Transform.up, Transform.position);
            
            if (plane.Raycast(ray, out var enter))
            {
                var point = ray.GetPoint(enter);
                
                if (IsInRange(point, clamp))
                {
                    var grid2D = WorldToGrid2(point, clamp);
                    
                    gridPos = new Vector3Int(grid2D.x, grid2D.y, activeDepth);
                    
                    return true;
                }
            }
            
            return false;
        }
    }
    
    public partial class WorldGrid<T> where T : struct, IGridNodeData
    {
        public virtual List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos, bool allowDiagonalCornerCutting = false)
        {
            var totalSize = Width * Height * Depth;
            var pathNodeArray = new T[totalSize];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        pathNodeArray[GridPositionToIndex(pos)] = GetGridObject(pos);
                    }
                }
            }

            var pathFinder = new FindPath<T>
            {
                GridSize = new int3(Width, Height, Depth),
                PathNodeArray = pathNodeArray,
                StartPos = startPos,
                EndPos = endPos,
                AllowDiagonalCornerCutting = allowDiagonalCornerCutting
            };

            return pathFinder.Execute();
        }
        public virtual NativeList<Vector3Int> FindPathWithJobs(Vector3Int startPos, Vector3Int endPos, bool allowDiagonalCornerCutting = false)
        {
            var totalSize = Width * Height * Depth;
            var pathNodeArray = new NativeArray<T>(totalSize, Allocator.TempJob);
            var result = new NativeList<Vector3Int>(Allocator.TempJob);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        pathNodeArray[GridPositionToIndex(new Vector3Int(x,y,z))] = GetGridObject(pos);
                    }
                }
            }

            var job = new FindPathJob<T>
            {
                PathNodeArray = pathNodeArray,
                GridSize = new int3(Width, Height, Depth),
                StartPos = startPos,
                EndPos = endPos,
                CalculatedPathList = result,
                AllowDiagonalCornerCutting = allowDiagonalCornerCutting
            };

            job.Schedule().Complete();
            pathNodeArray.Dispose();
            return result;
        }

        public int GridPositionToIndex(Vector3Int pos) => (pos.z * Width * Height) + (pos.y * Width) + pos.x;

        public Vector3Int IndexToGridPosition(int index)
        {
            var x = index % Width;
            
            var y = (index / Width) % Height;
            
            var z = index / (Width * Height);
            
            return new Vector3Int(x, y, z);
        }
    }
    
    public partial class WorldGrid<T> where T : struct, IGridNodeData
    {
        public virtual void DrawHighlightedCell(Vector3Int gridPos, Color highlightColor)
        {
            var center = GridToWorld(gridPos);
            var prevColor = Gizmos.color;

            Gizmos.color = highlightColor;

            var halfSize = new Vector3(
                CellSize.x * 0.5f,
                CellSize.y * 0.5f,
                CellSize.z * 0.5f);

            DrawCellGizmo(center, halfSize);

            Gizmos.color = prevColor;
        }

        public virtual void DrawGrid()
        {
            if (!DrawGizmos) return;

            Gizmos.color = GizmosColor;

            var halfSize = new Vector3(
                CellSize.x * 0.5f,
                CellSize.y * 0.5f,
                CellSize.z * 0.5f);

            int total = Width * Height * Depth;

            for (int i = 0; i < total; i++)
            {
                int x = i % Width;
                int y = (i / Width) % Height;
                int z = i / (Width * Height);

                var pos = new Vector3Int(x, y, z);
                var center = GridToWorld(pos);
                DrawCellGizmo(center, halfSize);
            }
        }

        private void DrawCellGizmo(Vector3 center, Vector3 halfSize)
        {
            Vector3 right = Transform.right * halfSize.x;
            Vector3 up = Transform.forward * halfSize.y;
            Vector3 forward = Transform.up * halfSize.z;

            Vector3[] corners = new Vector3[8];

            corners[0] = center - right - up - forward;
            corners[1] = center - right - up + forward;
            corners[2] = center - right + up - forward;
            corners[3] = center - right + up + forward;
            corners[4] = center + right - up - forward;
            corners[5] = center + right - up + forward;
            corners[6] = center + right + up - forward;
            corners[7] = center + right + up + forward;
            
            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[1], corners[5]);
            Gizmos.DrawLine(corners[5], corners[4]);
            Gizmos.DrawLine(corners[4], corners[0]);
            
            Gizmos.DrawLine(corners[2], corners[3]);
            Gizmos.DrawLine(corners[3], corners[7]);
            Gizmos.DrawLine(corners[7], corners[6]);
            Gizmos.DrawLine(corners[6], corners[2]);
            
            Gizmos.DrawLine(corners[0], corners[2]);
            Gizmos.DrawLine(corners[1], corners[3]);
            Gizmos.DrawLine(corners[4], corners[6]);
            Gizmos.DrawLine(corners[5], corners[7]);
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
            var cellCount = Width * Height * Depth;

            MeshExtensions.CreateEmptyMeshArrays3D(cellCount, out var vertices, out var uv, out var triangles);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        var node = GetGridObject(pos);
                        if (node.Equals(default(T))) continue;
                        var size = node.IsWalkable ? Vector3.zero : CellSize;
                        var worldPos = GridToWorld(pos);
                        var index = GridPositionToIndex(pos);
                        MeshExtensions.AddToMeshArrays3D(vertices, uv, triangles, index, worldPos, size, Vector2.zero, Vector2.zero, Transform);
                    }
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
        }
    }
    
    public partial class WorldGrid<T> where T : struct, IGridNodeData
    {
        public Vector3 GridToWorld2(Vector2Int gridPos, int depth = 0)
        {
            var x = (gridPos.x - ((Width - 1) / 2f)) * (CellSize.x + CellOffset.x) + GridOffset.x;
            var y = depth * (CellSize.z + CellOffset.z) + GridOffset.z;
            var z = (gridPos.y - ((Height - 1) / 2f)) * (CellSize.y + CellOffset.y) + GridOffset.y;
            return Transform.TransformPoint(new Vector3(x, y, z));
        }

        public virtual Vector3 GridToWorld(Vector3Int gridPos)
        {
            var x = (gridPos.x - ((Width - 1) / 2f)) * (CellSize.x + CellOffset.x) + GridOffset.x;
            var y = gridPos.z * (CellSize.z + CellOffset.z) + GridOffset.z;
            var z = (gridPos.y - ((Height - 1) / 2f)) * (CellSize.y + CellOffset.y) + GridOffset.y;
            return Transform.TransformPoint(new Vector3(x, y, z));
        }

        public Vector2Int WorldToGrid2(Vector3 position, bool clamp = false)
        {
            var grid = WorldToGrid(position, clamp);
            return grid.x == -1 ? new Vector2Int(-1, -1) : new Vector2Int(grid.x, grid.y);
        }

        public virtual Vector3Int WorldToGrid(Vector3 position, bool clamp = false)
        {
            position = Transform.InverseTransformPoint(position);

            var stepX = CellSize.x + CellOffset.x;
            var stepY = CellSize.y + CellOffset.y;
            var stepZ = CellSize.z + CellOffset.z;

            var halfGridWidth = (Width - 1) * stepX * 0.5f;
            var halfGridHeight = (Height - 1) * stepY * 0.5f;
            
            var x = Mathf.RoundToInt((position.x - GridOffset.x + halfGridWidth) / stepX);
            var y = Mathf.RoundToInt((position.z - GridOffset.y + halfGridHeight) / stepY);
            var z = Mathf.RoundToInt((position.y - GridOffset.z) / stepZ);
            
            if (clamp)
            {
                x = Mathf.Clamp(x, 0, Width - 1);
                y = Mathf.Clamp(y, 0, Height - 1);
                z = Mathf.Clamp(z, 0, Depth - 1);
                
                var gridPos = new Vector3Int(x, y, z);

                return gridPos;
            }
            
            var pos = new Vector3Int(x, y, z);

            if (!IsInRange(pos))
                return new Vector3Int(-1, -1, -1);

            var center = GridToWorld(pos);
            var localCenter = Transform.InverseTransformPoint(center);
            var localDelta = position - localCenter;
     
            if (Mathf.Abs(localDelta.x) - CellSize.x * 0.5f > 0f || Mathf.Abs(localDelta.z) - CellSize.y * 0.5f > 0f)
            {
                return new Vector3Int(-1, -1, -1);
            }
            
            return pos;
        }

        public bool IsInRange2(Vector2Int pos) => pos is { x: >= 0, y: >= 0 } && pos.x < Width && pos.y < Height;
        public bool IsInRange(Vector3Int pos) => pos is { x: >= 0, y: >= 0, z: >= 0 } && pos.x < Width && pos.y < Height && pos.z < Depth;
        public bool IsInRange2(Vector3 worldPos, bool clamp = false)
        {
            var grid = WorldToGrid2(worldPos, clamp);
            return grid.x != -1 && IsInRange2(grid);
        }

        public bool IsInRange(Vector3 worldPos, bool clamp = false)
        {
            var grid = WorldToGrid(worldPos, clamp);
            return grid.x != -1 && IsInRange(grid);
        }
    }
    
    public partial class WorldGrid<T> where T : struct, IGridNodeData
    {
        public T GetGridObject(Vector3Int gridPos)
        {
            if (_itemList.TryGetValue(gridPos, out var result) && result.Count > 0)
            {
                return result[0];
            }

            return default;
        }
        
        public T GetGridObject(Vector3 worldPos)
        {
            var worldToGrid3 = WorldToGrid(worldPos);
            
            return GetGridObject(worldToGrid3);
        }

        public void SetGridObject(Vector3Int gridPos, T item)
        {
            if (_itemList.TryGetValue(gridPos, out var result) && result.Count > 0)
            {
                result[0] = item;
            }
            else
            {
                _itemList.Add(gridPos, new List<T>() { item });
            }
        }
        
        public bool TryGet(Vector3Int gridPos, out IReadOnlyList<T> result)
        {
            if (_itemList.TryGetValue(gridPos, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        public bool TryGet(Vector3 worldPos, out IReadOnlyList<T> result)
        {
            return TryGet(WorldToGrid(worldPos), out result);
        }

        public void Add(Vector3Int gridPos, T item)
        {
            if (_itemList.TryGetValue(gridPos, out var result))
            {
                result.Add(item);
            }
            else
            {
                _itemList.Add(gridPos, new List<T>() { item });
            }
        }

        public bool Remove(Vector3Int gridPos, T item)
        {
            if (!_itemList.TryGetValue(gridPos, out var result))
            {
                return false;
            }

            if (!result.Contains(item))
            {
                return false;
            }
            
            result.Remove(item);
            
            return true;
        }

        public void Add(Vector3 worldPos, T item)
        {
            Add(WorldToGrid(worldPos), item);
        }

        public bool Remove(Vector3 worldPos, T item)
        {
            return Remove(WorldToGrid(worldPos), item);
        }

        public void ClearAll() => _itemList.Clear();
    }
}