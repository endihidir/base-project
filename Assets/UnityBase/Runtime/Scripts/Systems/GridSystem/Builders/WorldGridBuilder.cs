using UnityEngine;

namespace UnityBase.GridSystem
{
    public class WorldGridBuilder<T> where T : struct, IGridNodeData
    {
        protected Transform Transform;
        protected int? Width, Height, Depth;
        protected Vector3? CellSize;
        protected Vector3 Offset;
        protected bool DrawGizmos;
        protected Color GizmosColor = Color.white;
        protected Vector3 CellOffset = Vector3.one;

        public WorldGridBuilder<T> WithTransform(Transform transform) { Transform = transform; return this; }
        public WorldGridBuilder<T> WithSize(int width, int height, int depth) { Width = width; Height = height; Depth = depth; return this; }
        public WorldGridBuilder<T> WithCellSize(Vector3 cellSize) { CellSize = cellSize; return this; }
        public WorldGridBuilder<T> WithOffset(Vector3 offset) { Offset = offset; return this; }
        public WorldGridBuilder<T> WithGizmos(bool draw, Color color) { DrawGizmos = draw; GizmosColor = color; return this; }
        public WorldGridBuilder<T> WithCellOffset(Vector3 cellSpace) { CellOffset = cellSpace; return this; }

        public virtual IWorldGrid<T> Build()
        {
            if (!Transform || !Width.HasValue || !Height.HasValue || !Depth.HasValue || !CellSize.HasValue)
            {
                Debug.LogError("WorldGridBuilder: Missing required parameters.");
                return default;
            }

            return new WorldGrid<T>(Transform, Width.Value, Height.Value, Depth.Value, CellSize.Value, Offset,CellOffset, DrawGizmos, GizmosColor);
        }
    }
    
    public class HexWorldGridBuilder<T> : WorldGridBuilder<T> where T : struct, IGridNodeData
    {
        private bool _isPointyTopped = true;

        public HexWorldGridBuilder<T> WithIsPointyTopped(bool isPointyTopped)
        {
            _isPointyTopped = isPointyTopped;
            return this;
        }

        public override IWorldGrid<T> Build()
        {
            if (Transform == null || !Width.HasValue || !Height.HasValue || !Depth.HasValue || !CellSize.HasValue)
            {
                Debug.LogError("HexWorldGridBuilder: Missing required parameters.");
                return default;
            }

            return new HexWorldGrid<T>(Transform, Width.Value, Height.Value, Depth.Value, CellSize.Value, Offset, CellOffset, DrawGizmos, GizmosColor, _isPointyTopped);
        }
    }
}