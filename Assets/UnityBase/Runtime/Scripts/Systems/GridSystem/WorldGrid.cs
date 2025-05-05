using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityBase.PathFinding;
using UnityEngine;

namespace UnityBase.GridSystem
{
    public interface IWorldGrid<T> : IGrid where T : struct
    {
        int Depth { get; }
        bool DrawGizmos { get; }
        Transform Transform { get; }

        void Initialize(Func<Vector3Int, T> generator);
        public void Update(int width, int height, int depth, Vector3 cellSize, Vector3 offset, Vector3 cellOffset,bool drawGizmos, Color gizmosColor);

        void Add(Vector3Int gridPos, T item);
        bool Remove(Vector3Int gridPos, T item);
        void Add(Vector3 worldPos, T item);
        bool Remove(Vector3 worldPos, T item);

        void SetFirst(Vector3Int gridPos, T item);
        T GetFirst(Vector3Int gridPos);
        T GetFirst(Vector3 worldPos);

        bool TryGet(Vector3Int gridPos, out IReadOnlyList<T> result);
        bool TryGet(Vector3 worldPos, out IReadOnlyList<T> result);
        bool TryGetNodeFromScreenRay(Ray ray, int activeDepth, out Vector3Int gridPos);

        int TryGetNeighbors(Vector3Int gridPos, int size, T[] resultBuffer, bool includeSelf = false, bool includeDepth = false, bool includeDiagonal = true);
        int TryGetImmediateNeighborsNonAlloc(Vector3Int gridPos, Span<T> resultBuffer, bool includeSelf = false, bool includeDepth = false, bool includeDiagonal = true);

        bool TryGetNeighbor(Vector3Int pos, Direction direction, out T neighbor, bool includeDepth = false, bool includeDiagonal = false);

        Vector3 GridToWorld(Vector2Int gridPos, int depth = 0);
        Vector3 GridToWorld(Vector3Int gridPos);
        Vector2Int WorldToGrid2(Vector3 position, bool clamp = true);
        Vector3Int WorldToGrid3(Vector3 position, bool clamp = true);

        bool IsInRange2(Vector2Int pos);
        bool IsInRange3(Vector3Int pos);
        bool IsInRange2(Vector3 worldPos);
        bool IsInRange3(Vector3 worldPos);
        
        int CalculateIndex(Vector3Int pos);
        Vector3Int ReverseCalculateIndex(int index);
        
        void DrawHighlightedCell(Vector3Int gridPos, Color highlightColor);
        void DrawGrid();
        public void DebugDrawPath(List<Vector3Int> path, float duration, Color color);
        void DebugDrawPath(NativeList<Vector3Int> path, float duration, Color color);
        void RebuildMeshVisual(Mesh mesh);

        List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos, bool allowDiagonalCornerCutting = false);
        NativeList<Vector3Int> FindPathWithJobs(Vector3Int startPos, Vector3Int endPos, bool allowDiagonalCornerCutting = false);
        void ClearAll();
    }
    
    public partial class WorldGrid<T> : IWorldGrid<T> where T : struct, IPathNodeData
    {
        private int _gridWidth;
        private int _gridHeight;
        private int _gridDepth;
        private bool _drawGizmos;
        private Vector3 _gridCellSize;
        private Vector3 _gridOffset;
        private Vector3 _cellOffset;
        private Color _gizmosColor;
        
        private readonly Dictionary<Vector3Int, List<T>> _itemList = new();
        
        private static readonly ConcurrentDictionary<(bool,bool), Vector3Int[]> _offsetCache = new();
        
        private static readonly Dictionary<Direction, Vector3Int> _baseDirections = new()
        {
            { Direction.Right, new Vector3Int(1, 0, 0) },
            { Direction.Left, new Vector3Int(-1, 0, 0) },
            { Direction.Up, new Vector3Int(0, 1, 0) },
            { Direction.Down, new Vector3Int(0, -1, 0) },
            { Direction.Forward, new Vector3Int(0, 0, 1) },
            { Direction.Backward, new Vector3Int(0, 0, -1) }
        };
        
        private static readonly Vector3Int[] _neighborOffsets = 
                Enumerable.Range(-1, 3).SelectMany(x => Enumerable.Range(-1, 3)
                .SelectMany(y => Enumerable.Range(-1, 3).Where(z => x != 0 || y != 0 || z != 0).Select(z => new Vector3Int(x, y, z))))
                .ToArray();
        
        private Transform _transform;
        
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
            _gizmosColor = gizmosColor;
            _cellOffset = cellOffset;
        }
        
        public void Initialize(Func<Vector3Int, T> generator)
        {
            _itemList.Clear();
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            for (int z = 0; z < Depth; z++)
            {
                var pos = new Vector3Int(x, y, z);
                var data = generator.Invoke(pos);
                SetFirst(pos, data);
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
            _gizmosColor = gizmosColor;
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

        public int TryGetNeighbors(Vector3Int gridPos, int size, T[] resultBuffer, bool includeSelf = false, bool includeDepth = false, bool includeDiagonal = true)
        {
            var yBegin = gridPos.y - size;
            var yEnd = gridPos.y + size;

            var xBegin = gridPos.x - size;
            var xEnd = gridPos.x + size;

            var zBegin = gridPos.z - size;
            var zEnd = gridPos.z + size;

            if (includeDepth)
            {
                zBegin = Mathf.Max(0, gridPos.z - size);
                zEnd = Mathf.Min(_gridDepth - 1, gridPos.z + size);
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
                        
                        if (!IsInRange3(pos)) continue;

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
        
        public int TryGetImmediateNeighborsNonAlloc(Vector3Int gridPos, Span<T> resultBuffer, bool includeSelf = false, bool includeDepth = false, bool includeDiagonal = true)
        {
            var count = 0;

            foreach (var offset in GetFilteredOffsets(includeDepth, includeDiagonal))
            {
                var neighborPos = gridPos + offset;

                if (!includeSelf && neighborPos == gridPos) continue;

                if (!IsInRange3(neighborPos)) continue;

                if (!_itemList.TryGetValue(neighborPos, out var items)) continue;

                foreach (var item in items)
                {
                    if (count >= resultBuffer.Length)
                    {
                        Debug.LogError("TryGetImmediateNeighborsNonAlloc resultBuffer capacity full");
                        return count;
                    }

                    resultBuffer[count++] = item;
                }
            }

            return count;
        }
        
        public bool TryGetNeighbor(Vector3Int pos, Direction direction, out T neighbor, bool includeDepth = false, bool includeDiagonal = false)
        {
            neighbor = default;

            if (_baseDirections.TryGetValue(direction, out var baseDir))
            {
                var basePos = pos + baseDir;

                if (IsInRange3(basePos))
                {
                    var baseItem = GetFirst(basePos);
                    
                    if (!baseItem.Equals(default(T)))
                    {
                        neighbor = baseItem;
                        return true;
                    }
                }
            }

            foreach (var offset in GetFilteredOffsets(includeDepth, includeDiagonal))
            {
                var neighborPos = pos + offset;

                if (!IsInRange3(neighborPos)) continue;

                var item = GetFirst(neighborPos);
                
                if (!item.Equals(default(T)))
                {
                    neighbor = item;
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<Vector3Int> GetFilteredOffsets(bool includeDepth, bool includeDiagonal)
        {
            var key = (includeDepth, includeDiagonal);
            
            return _offsetCache.GetOrAdd(key, k => 
                _neighborOffsets.Where(offset => (k.Item1 || offset.z == 0) && 
                                                 (k.Item2 || (Mathf.Abs(offset.x) + Mathf.Abs(offset.y) + Mathf.Abs(offset.z)) <= 1)).ToArray());
        }
        
        public bool TryGetNodeFromScreenRay(Ray ray, int activeDepth, out Vector3Int gridPos)
        {
            gridPos = default;
            
            if (Depth <= activeDepth) return false;
            
            var plane = new Plane(Transform.up, Transform.position);
            
            if (plane.Raycast(ray, out var enter))
            {
                var point = ray.GetPoint(enter);
                
                if (IsInRange3(point))
                {
                    var grid2D = WorldToGrid2(point);
                    
                    gridPos = new Vector3Int(grid2D.x, grid2D.y, activeDepth);
                    
                    return true;
                }
            }
            
            return false;
        }
    }
    
    public partial class WorldGrid<T> where T : struct, IPathNodeData
    {
        public List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos, bool allowDiagonalCornerCutting = false)
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
                        pathNodeArray[CalculateIndex(pos)] = GetFirst(pos);
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
        public NativeList<Vector3Int> FindPathWithJobs(Vector3Int startPos, Vector3Int endPos, bool allowDiagonalCornerCutting = false)
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
                        pathNodeArray[CalculateIndex(new Vector3Int(x,y,z))] = GetFirst(pos);
                    }
                }
            }

            var job = new FindPathJob<T>
            {
                pathNodeArray = pathNodeArray,
                gridSize = new int3(Width, Height, Depth),
                startPos = startPos,
                endPos = endPos,
                calculatedPathList = result,
                allowDiagonalCornerCutting = allowDiagonalCornerCutting
            };

            job.Schedule().Complete();
            pathNodeArray.Dispose();
            return result;
        }

        public int CalculateIndex(Vector3Int pos) => (pos.z * Width * Height) + (pos.y * Width) + pos.x;

        public Vector3Int ReverseCalculateIndex(int index)
        {
            var x = index % Width;
            
            var y = (index / Width) % Height;
            
            var z = index / (Width * Height);
            
            return new Vector3Int(x, y, z);
        }
    }
    
    public partial class WorldGrid<T> where T : struct, IPathNodeData
    {
        public void DrawHighlightedCell(Vector3Int gridPos, Color highlightColor)
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

        public void DrawGrid()
        {
            if (!DrawGizmos || !Application.isPlaying) return;

            Gizmos.color = _gizmosColor;

            var halfSize = new Vector3(
                CellSize.x * 0.5f,
                CellSize.y * 0.5f,
                CellSize.z * 0.5f);

            for (int d = 0; d < Depth; d++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var pos = new Vector3Int(x, y, d);
                        var center = GridToWorld(pos);
                        DrawCellGizmo(center, halfSize);
                    }
                }
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

            // Bottom
            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[1], corners[5]);
            Gizmos.DrawLine(corners[5], corners[4]);
            Gizmos.DrawLine(corners[4], corners[0]);

            // Top
            Gizmos.DrawLine(corners[2], corners[3]);
            Gizmos.DrawLine(corners[3], corners[7]);
            Gizmos.DrawLine(corners[7], corners[6]);
            Gizmos.DrawLine(corners[6], corners[2]);

            // Sides
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

        public void RebuildMeshVisual(Mesh mesh)
        {
            var cellCount = Width * Height * Depth;

            MeshUtils.CreateEmptyMeshArrays3D(cellCount, out var vertices, out var uv, out var triangles);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        var node = GetFirst(pos);
                        if (node.Equals(default(T))) continue;
                        var size = node.IsWalkable ? Vector3.zero : CellSize;
                        var worldPos = GridToWorld(pos);
                        int index = CalculateIndex(pos);
                        MeshUtils.AddToMeshArrays3D(vertices, uv, triangles, index, worldPos, size, Vector2.zero, Vector2.zero, Transform);
                    }
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
        }
    }
    
    public partial class WorldGrid<T> where T : struct, IPathNodeData
    {
        public Vector3 GridToWorld(Vector2Int gridPos, int depth = 0)
        {
            var x = (gridPos.x - ((Width - 1) / 2f)) * (CellSize.x + CellOffset.x) + GridOffset.x;
            var y = (depth + 0.5f) * (CellSize.z + CellOffset.z) + GridOffset.z;
            var z = (gridPos.y - ((Height - 1) / 2f)) * (CellSize.y + CellOffset.y) + GridOffset.y;
            return Transform.TransformPoint(new Vector3(x, y, z));
        }

        public Vector3 GridToWorld(Vector3Int gridPos)
        {
            var x = (gridPos.x - ((Width - 1) / 2f)) * (CellSize.x + CellOffset.x) + GridOffset.x;
            var y = (gridPos.z + 0.5f) * (CellSize.z + CellOffset.z) + GridOffset.z;
            var z = (gridPos.y - ((Height - 1) / 2f)) * (CellSize.y + CellOffset.y) + GridOffset.y;
            return Transform.TransformPoint(new Vector3(x, y, z));
        }

        public Vector2Int WorldToGrid2(Vector3 position, bool clamp = true)
        {
            var grid = WorldToGrid3(position, clamp);
            return grid.x == -1 ? new Vector2Int(-1, -1) : new Vector2Int(grid.x, grid.y);
        }

        public Vector3Int WorldToGrid3(Vector3 position, bool clamp = true)
        {
            position = Transform.InverseTransformPoint(position);

            var stepX = CellSize.x + CellOffset.x;
            var stepY = CellSize.y + CellOffset.y;
            var stepZ = CellSize.z + CellOffset.z;

            var halfGridWidth = (Width - 1) * stepX * 0.5f;
            var halfGridHeight = (Height - 1) * stepY * 0.5f;
            var halfGridDepth = Depth * stepZ * 0.5f;
            
            var x = Mathf.RoundToInt((position.x - GridOffset.x + halfGridWidth) / stepX);
            var y = Mathf.RoundToInt((position.z - GridOffset.y + halfGridHeight) / stepY);
            var z = Mathf.FloorToInt((position.y - GridOffset.z + halfGridDepth) / stepZ);
            
            var gridPos = new Vector3Int(x, y, z);

            if (!IsInRange3(gridPos))
                return new Vector3Int(-1, -1, -1);

            var center = GridToWorld(gridPos);
            var localCenter = Transform.InverseTransformPoint(center);
            var localDelta = position - localCenter;
     
            if (Mathf.Abs(localDelta.x) - CellSize.x * 0.5f > 0f || 
                Mathf.Abs(localDelta.z) - CellSize.y * 0.5f > 0f)
            {
                return new Vector3Int(-1, -1, -1);
            }
            
            return gridPos;
        }

        public bool IsInRange2(Vector2Int pos) => pos is { x: >= 0, y: >= 0 } && pos.x < Width && pos.y < Height;
        public bool IsInRange3(Vector3Int pos) => pos is { x: >= 0, y: >= 0, z: >= 0 } && pos.x < Width && pos.y < Height && pos.z < Depth;
        public bool IsInRange2(Vector3 worldPos)
        {
            var grid = WorldToGrid2(worldPos, false);
            return grid.x != -1 && IsInRange2(grid);
        }

        public bool IsInRange3(Vector3 worldPos)
        {
            var grid = WorldToGrid3(worldPos, false);
            return grid.x != -1 && IsInRange3(grid);
        }
    }
    
    public partial class WorldGrid<T> where T : struct, IPathNodeData
    {
        public T GetFirst(Vector3Int gridPos)
        {
            if (_itemList.TryGetValue(gridPos, out var result) && result.Count > 0)
            {
                return result[0];
            }

            return default;
        }
        
        public T GetFirst(Vector3 worldPos)
        {
            var worldToGrid3 = WorldToGrid3(worldPos);
            
            return GetFirst(worldToGrid3);
        }

        public void SetFirst(Vector3Int gridPos, T item)
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
            return TryGet(WorldToGrid3(worldPos), out result);
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
            Add(WorldToGrid3(worldPos), item);
        }

        public bool Remove(Vector3 worldPos, T item)
        {
            return Remove(WorldToGrid3(worldPos), item);
        }

        public void ClearAll() => _itemList.Clear();
    }
    
    public class WorldGridBuilder<T> where T : struct, IPathNodeData
    {
        private Transform _transform;
        private int? _width, _height, _depth;
        private Vector3? _cellSize;
        private Vector3 _offset;
        private bool _drawGizmos;
        private Color _gizmosColor = Color.white;
        private Vector3 _cellOffset = Vector3.one;

        public WorldGridBuilder<T> WithTransform(Transform transform) { _transform = transform; return this; }
        public WorldGridBuilder<T> WithSize(int width, int height, int depth) { _width = width; _height = height; _depth = depth; return this; }
        public WorldGridBuilder<T> WithCellSize(Vector3 cellSize) { _cellSize = cellSize; return this; }
        public WorldGridBuilder<T> WithOffset(Vector3 offset) { _offset = offset; return this; }
        public WorldGridBuilder<T> WithGizmos(bool draw, Color color) { _drawGizmos = draw; _gizmosColor = color; return this; }
        public WorldGridBuilder<T> WithCellOffset(Vector3 cellSpace) { _cellOffset = cellSpace; return this; }

        public WorldGrid<T> Build()
        {
            if (!_transform || !_width.HasValue || !_height.HasValue || !_depth.HasValue || !_cellSize.HasValue)
            {
                Debug.LogError("WorldGridBuilder: Missing required parameters.");
                return default;
            }

            return new WorldGrid<T>(_transform, _width.Value, _height.Value, _depth.Value, _cellSize.Value, _offset,_cellOffset, _drawGizmos, _gizmosColor);
        }
    }
}