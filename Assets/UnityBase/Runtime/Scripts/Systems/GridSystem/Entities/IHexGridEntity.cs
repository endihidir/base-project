using Unity.Mathematics;

namespace UnityBase.GridSystem
{
    public interface IHexGridEntity
    {
        public bool Is2DGrid { get; }
        public bool UseVerticalShape { get; }
        public int Width { get; }
        public int Height { get; }
        public float2 NodeSize { get; }
        public float2 OffsetMultiplier { get; }
        public float3 OriginPos { get; }
    }
}