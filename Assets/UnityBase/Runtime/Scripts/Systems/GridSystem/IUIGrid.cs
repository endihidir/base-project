using System;
using UnityEngine;

namespace UnityBase.GridSystem
{
    public interface IUIGrid<T> : IGrid<T> where T : struct, IGridNodeData
    {
        bool TryGetGridObjectFromMousePosition(out T obj);
        bool TryFindPositionOf(T obj, out Vector3Int pos);
        bool TryGetNeighbor(T currentObject, Direction2D direction2D, out T neighbour);
        bool TryGetNeighbor(Vector3Int pos, Direction2D direction2D, out T neighbour);
        bool TryGetNeighbors(T currentObject, out T[] neighbours);
        bool TryGetNeighborsNonAlloc(T currentObject, Span<T> resultBuffer, out int count);
        public void Update(int width, int height, float screenSidePaddingRatio, float cellSpacingRatio, Vector3 originOffset, bool drawGizmos, Color gizmosColor);
    }
}