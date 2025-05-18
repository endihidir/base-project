using UnityEngine;

namespace UnityBase.GridSystem
{
    public class HexUIGridTest : UIGridTest
    {
        [SerializeField] private bool _isPointyTopped = true;
        protected override void Init()
        {
            _grid = new HexUIGrid<GridNode>(Camera.main, _width, _height, _screenSidePaddingRatio, _cellSpacingRatio, _originOffset, _drawGizmos, _gizmosColor, _isPointyTopped);
        }
    }
}