using System;
using System.Collections.Generic;
using Unity.Collections;
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

        Vector3 GridToWorld2(Vector2Int gridPos, int depth = 0);
        Vector3 GridToWorld3(Vector3Int gridPos);
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
}