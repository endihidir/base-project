using UnityEngine;

namespace UnityBase.GridSystem
{
    public class HexWorldGridTest : WorldGridTest
    {
        [SerializeField] private bool _isPointyTopped = true;
        
        protected override void Init()
        {
            _grid = new HexWorldGridBuilder<GridNode>()
                .WithIsPointyTopped(_isPointyTopped)
                .WithTransform(transform)
                .WithSize(_gridWidth, _gridHeight, _gridDepth)
                .WithCellSize(_cellSize)
                .WithOffset(_gridOffset)
                .WithCellOffset(_cellOffset)
                .WithGizmos(_drawGizmos, _gizmosColor)
                .Build();
        }
    }
}