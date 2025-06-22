using UnityEngine;

namespace UnityBase.GridSystem
{
    public class UIGridBuilder<T> where T : struct, IGridNodeData
    {
        protected Camera _camera;
        protected int _width = 10;
        protected int _height = 10;
        protected float _screenSidePaddingRatio = 5f;
        protected float _cellSpacingRatio = 1f;
        protected Vector3 _originOffset = Vector3.zero;
        protected bool _drawGizmos = true;
        protected Color _gizmosColor = Color.white;

        public UIGridBuilder<T> WithCamera(Camera camera)
        {
            _camera = camera;
            return this;
        }

        public UIGridBuilder<T> WithSize(int width, int height)
        {
            _width = width;
            _height = height;
            return this;
        }

        public UIGridBuilder<T> WithScreenSidePaddingRatio(float paddingRatio)
        {
            _screenSidePaddingRatio = paddingRatio;
            return this;
        }

        public UIGridBuilder<T> WithCellSpacingRatio(float spacingRatio)
        {
            _cellSpacingRatio = spacingRatio;
            return this;
        }

        public UIGridBuilder<T> WithOriginOffset(Vector3 offset)
        {
            _originOffset = offset;
            return this;
        }

        public UIGridBuilder<T> WithGizmos(bool drawGizmos, Color gizmosColor)
        {
            _drawGizmos = drawGizmos;
            _gizmosColor = gizmosColor;
            return this;
        }

        public virtual IUIGrid<T> Build()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                Debug.LogWarning("No camera was specified for UIGrid. Using Camera.main by default.");
            }

            return new UIGrid<T>(
                _camera,
                _width,
                _height,
                _screenSidePaddingRatio,
                _cellSpacingRatio,
                _originOffset,
                _drawGizmos,
                _gizmosColor
            );
        }
    }
    
    public class HexUIGridBuilder<T> : UIGridBuilder<T> where T : struct, IGridNodeData
    {
        private bool _isPointyTopped = true;

        public HexUIGridBuilder<T> WithIsPointyTopped(bool isPointyTopped)
        {
            _isPointyTopped = isPointyTopped;
            return this;
        }

        public override IUIGrid<T> Build()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                Debug.LogWarning("No camera was specified for HexUIGrid. Using Camera.main by default.");
            }

            return new HexUIGrid<T>(
                _camera,
                _width,
                _height,
                _screenSidePaddingRatio,
                _cellSpacingRatio,
                _originOffset,
                _drawGizmos,
                _gizmosColor,
                _isPointyTopped
            );
        }
    }
}
