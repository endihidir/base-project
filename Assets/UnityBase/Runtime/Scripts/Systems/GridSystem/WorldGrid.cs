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
     public interface IWorldGrid<T> where T : struct
    {
        float CellSize { get; }
        float CellDepth { get; }
        int Width { get; }
        int Height { get; }
        int Depth { get; }
        bool DrawGizmos { get; }
        Transform Transform { get; }

        void Initialize(Func<Vector3Int, T> generator);
        void Update(int width, int height, int depth, float cellSize, float cellDepth, Vector3 offset, bool drawGizmos, Color gizmosColor);

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
        private float _gridCellSize;
        private float _gridCellDepth;
        private Vector3 _gridOffset;
        private bool _drawGizmos;
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
      
        public IReadOnlyDictionary<Vector3Int, List<T>> Cells => _itemList;
        
        public float CellSize => _gridCellSize;
        public float CellDepth => _gridCellDepth;

        public int Width => _gridWidth;
        public int Height => _gridHeight;
        public int Depth => _gridDepth;
        public bool DrawGizmos => _drawGizmos;

        public Transform Transform => _transform;
        
        public WorldGrid(Transform coreTransform, int width, int height, int depth, float cellSize, float cellDepth, Vector3 offset, bool drawGizmos, Color gizmosColor)
        {
            _transform = coreTransform;
            _gridWidth = width;
            _gridHeight = height;
            _gridDepth = depth;
            _gridCellSize = cellSize;
            _gridCellDepth = cellDepth;
            _gridOffset = offset;
            _drawGizmos = drawGizmos;
            _gizmosColor = gizmosColor;
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

        public void Update(int width, int height, int depth, float cellSize, float cellDepth, Vector3 offset, bool drawGizmos, Color gizmosColor)
        {
            _gridWidth = width;
            _gridHeight = height;
            _gridDepth = depth;
            _gridCellSize = cellSize;
            _gridCellDepth = cellDepth;
            _gridOffset = offset;
            _drawGizmos = drawGizmos;
            _gizmosColor = gizmosColor;
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
            
            gridPos = default;
            
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
        
        public int CalculateIndex(Vector3Int pos) => (pos.z * _gridWidth * _gridHeight) + (pos.y * _gridWidth) + pos.x;

        public Vector3Int ReverseCalculateIndex(int index)
        {
            var x = index % _gridWidth;
            
            var y = (index / _gridWidth) % _gridHeight;
            
            var z = index / (_gridWidth * _gridHeight);
            
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
            DrawCellGizmo(center, _gridCellSize * 0.55f);
    
            Gizmos.color = prevColor;
        }
        
        public void DrawGrid()
        {
            if (!DrawGizmos || !Application.isPlaying) return;

            Gizmos.color = _gizmosColor;
            var halfSize = _gridCellSize * 0.5f;
    
            for (int d = 0; d < _gridDepth; d++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    for (int x = 0; x < _gridWidth; x++)
                    {
                        var center = GridToWorld(new Vector2Int(x, y), d);
                        DrawCellGizmo(center, halfSize);
                    }
                }
            }
        }

        private void DrawCellGizmo(Vector3 center, float halfSize)
        {
            var forward = _transform.forward * halfSize;
            var right = _transform.right * halfSize;
    
            var p1 = center - right - forward;
            var p2 = center - right + forward;
            var p3 = center + right + forward;
            var p4 = center + right - forward;

            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);
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
            
            MeshUtils.CreateEmptyMeshArrays(cellCount, out var vertices, out var uv, out var triangles);

            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            for (int z = 0; z < Depth; z++)
            {
                var pos = new Vector3Int(x, y, z);
                var node = GetFirst(pos);
                var size = node.IsWalkable ? Vector3.zero : Vector3.one * CellSize;
                var worldPos = GridToWorld(pos);
                int index = CalculateIndex(pos);
                MeshUtils.AddToMeshArrays2(vertices, uv, triangles, index, worldPos, size, Vector2.zero, Vector2.zero, Transform);
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
        }
        
    }
    
    public partial class WorldGrid<T> where T : struct, IPathNodeData
    {
        public Vector3 GridToWorld(Vector2Int gridPos, int depth = 0)
        {
            var x = (gridPos.x - (Width / 2f)) * CellSize + _gridOffset.x;
            var y = (depth - Depth / 2f) * CellDepth + _gridOffset.y;
            var z = (gridPos.y - (Height / 2f)) * CellSize + _gridOffset.z;
            return Transform.TransformPoint(new Vector3(x, y, z));
        }

        public Vector3 GridToWorld(Vector3Int gridPos)
        {
            var x = (gridPos.x - (Width / 2f)) * CellSize + _gridOffset.x;
            var y = (gridPos.z - Depth / 2f) * CellDepth + _gridOffset.y;
            var z = (gridPos.y - (Height / 2f)) * CellSize + _gridOffset.z;
            return Transform.TransformPoint(new Vector3(x, y, z));
        }

        public Vector2Int WorldToGrid2(Vector3 position, bool clamp = true) => new(WorldToGrid3(position, clamp).x, WorldToGrid3(position, clamp).y);

        public Vector3Int WorldToGrid3(Vector3 position, bool clamp = true)
        {
            position = Transform.InverseTransformPoint(position);
            var cellOffsetX = Width % 2 == 0 ? CellSize * .5f : 0;
            var cellOffsetY = Height % 2 == 0 ? CellSize * .5f : 0;
            var cellOffsetD = CellDepth % 2 == 0 ? CellDepth * .5f : 0;

            var x = Width - Mathf.CeilToInt(Width / 2f - ((position.x - (_gridOffset.x) + cellOffsetX) / CellSize));
            var y = Height - Mathf.CeilToInt(Height / 2f - ((position.z - (_gridOffset.z) + cellOffsetY) / CellSize));
            var d = Depth - Mathf.CeilToInt(Depth / 2f - ((position.y - (_gridOffset.y) + cellOffsetD) / CellDepth));

            if (clamp)
            {
                x = Mathf.Clamp(x, 0, Width - 1);
                y = Mathf.Clamp(y, 0, Height - 1);
                d = Mathf.Clamp(d, 0, Depth - 1);
            }

            return new Vector3Int(x, y, d);
        }

        public bool IsInRange2(Vector2Int pos) => pos is { x: >= 0, y: >= 0 } && pos.x < Width && pos.y < Height;
        public bool IsInRange3(Vector3Int pos) => pos is { x: >= 0, y: >= 0, z: >= 0 } && pos.x < Width && pos.y < Height && pos.z < Depth;
        public bool IsInRange2(Vector3 worldPos) => IsInRange2(WorldToGrid2(worldPos, false));
        public bool IsInRange3(Vector3 worldPos) => IsInRange3(WorldToGrid3(worldPos, false));
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
        private float? _cellSize, _cellDepth;
        private Vector3 _offset;
        private bool _drawGizmos;
        private Color _gizmosColor = Color.white;

        public WorldGridBuilder<T> WithTransform(Transform transform) { _transform = transform; return this; }
        public WorldGridBuilder<T> WithSize(int width, int height, int depth) { _width = width; _height = height; _depth = depth; return this; }
        public WorldGridBuilder<T> WithCellSize(float size, float depth) { _cellSize = size; _cellDepth = depth; return this; }
        public WorldGridBuilder<T> WithOffset(Vector3 offset) { _offset = offset; return this; }
        public WorldGridBuilder<T> WithGizmos(bool draw, Color color) { _drawGizmos = draw; _gizmosColor = color; return this; }

        public WorldGrid<T> Build()
        {
            if (!_transform || !_width.HasValue || !_height.HasValue || !_depth.HasValue || !_cellSize.HasValue || !_cellDepth.HasValue)
            {
                Debug.LogError("WorldGridBuilder: Missing required parameters.");
                return default;
            }

            return new WorldGrid<T>(_transform, _width.Value, _height.Value, _depth.Value, _cellSize.Value, _cellDepth.Value, _offset, _drawGizmos, _gizmosColor);
        }
    }
}