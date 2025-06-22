using UnityEngine;

namespace UnityBase.GridSystem
{
    public class HexUIGridTest : UIGridTest
    {
        [SerializeField] private bool _isPointyTopped = true;
        protected override void Init()
        {
            _grid = new HexUIGridBuilder<GridNode>()
                .WithIsPointyTopped(_isPointyTopped)
                .WithCamera(Camera.main)
                .WithSize(_width, _height)
                .WithScreenSidePaddingRatio(_screenSidePaddingRatio)
                .WithCellSpacingRatio(_cellSpacingRatio)
                .WithOriginOffset(_originOffset)
                .WithGizmos(_drawGizmos, _gizmosColor)
                .Build();
        }
    }
}