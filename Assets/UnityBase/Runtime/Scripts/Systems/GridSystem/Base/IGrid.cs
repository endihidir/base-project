using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UnityBase.GridSystem
{
    public interface IGrid<T> where T : struct
    {
        int Width { get; }
        int Height { get; }
        bool DrawGizmos { get; }
        T GetGridObject(Vector3Int gridPos);
        void SetGridObject(Vector3Int gridPos, T item);
        T GetGridObject(Vector3 worldPos);
        Vector3 GridToWorld(Vector3Int pos);
        Vector3Int WorldToGrid(Vector3 position, bool clamp = true);
        bool IsInRange(Vector3Int pos);
        int GridPositionToIndex(Vector3Int pos);
        Vector3Int IndexToGridPosition(int index);
        void DrawGrid();
        void RebuildMeshVisual(Mesh mesh);
        List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos, bool allowDiagonal = false);
        NativeList<Vector3Int> FindPathWithJobs(Vector3Int startPos, Vector3Int endPos, bool allowDiagonal = false);
        void DebugDrawPath(List<Vector3Int> path, float duration, Color color);
        void DebugDrawPath(NativeList<Vector3Int> path, float duration, Color color);
    }
}