using Unity.Mathematics;

namespace UnityBase.GridSystem
{
    public interface IGridEntity
    {
        public bool Is2DGrid { get; }
        public int Width { get; }
        public int Height { get; }
        public float NodeSize { get; }
        public float3 Padding { get; }
        public float3 OriginPos { get; }
    }
}