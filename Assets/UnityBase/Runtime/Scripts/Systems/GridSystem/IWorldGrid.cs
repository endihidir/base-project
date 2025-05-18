using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityBase.GridSystem
{
    public interface IWorldGrid<T> : IGrid<T> where T : struct
    {
        Vector3 CellSize { get; }  
        Vector3 CellOffset { get; }
        Vector3 GridOffset { get; }
        int Depth { get; }
        Transform Transform { get; }
        
        public void Update(int width, int height, int depth, Vector3 cellSize, Vector3 offset, Vector3 cellOffset,bool drawGizmos, Color gizmosColor);

        void Add(Vector3Int gridPos, T item);
        bool Remove(Vector3Int gridPos, T item);
        void Add(Vector3 worldPos, T item);
        bool Remove(Vector3 worldPos, T item);

        bool TryGet(Vector3Int gridPos, out IReadOnlyList<T> result);
        bool TryGet(Vector3 worldPos, out IReadOnlyList<T> result);
        bool TryGetNodeFromScreenRay(Ray ray, int activeDepth, out Vector3Int gridPos);

        int TryGetNeighbors(Vector3Int gridPos, int size, T[] resultBuffer, bool includeSelf = false, bool includeDepth = false, bool includeDiagonal = true);
        int TryGetImmediateNeighborsNonAlloc(Vector3Int gridPos, Span<T> resultBuffer, bool includeSelf = false, bool includeDepth = false, bool includeDiagonal = true);
        bool TryGetNeighbor(Vector3Int pos, Direction direction, out T neighbor, bool includeDepth = false, bool includeDiagonal = false);

        Vector3 GridToWorld2(Vector2Int gridPos, int depth = 0);
        Vector2Int WorldToGrid2(Vector3 position, bool clamp = true);
        
        bool IsInRange(Vector3 worldPos);
        bool IsInRange2(Vector2Int pos);
        bool IsInRange2(Vector3 worldPos);
        
        void DrawHighlightedCell(Vector3Int gridPos, Color highlightColor);
        void ClearAll();
    }
}