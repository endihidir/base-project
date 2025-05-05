using UnityEngine;

namespace UnityBase.GridSystem
{
    public interface IGrid
    {
        int Width { get; }
        int Height { get; }
        Vector3 CellSize { get; }  
        Vector3 CellOffset { get; }
        Vector3 GridOffset { get; }
    }
}