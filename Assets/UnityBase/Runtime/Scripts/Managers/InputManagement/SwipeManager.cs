using UnityBase.Service;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityBase.Manager
{
    public class SwipeManager : ISwipeManager
    {
        private readonly float _minDistanceForSwipe = Screen.width * 0.1f;
        private Vector2 _fingerDownPosition, _fingerUpPosition;
        private Direction2D _direction2D;
        private bool _isDragging;

        public void Initialize() { }
        public void Dispose() { }

        public Direction2D GetSwipeDirection()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                ResetInput();
                return _direction2D;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _fingerDownPosition = Input.mousePosition;
                _fingerUpPosition = Input.mousePosition;
            }

            if (_isDragging && Input.GetMouseButton(0))
            {
                _fingerUpPosition = Input.mousePosition;
                CheckSwipe();
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
                _fingerUpPosition = Input.mousePosition;
                CheckSwipe();
            }

            return _direction2D;
        }

        private void CheckSwipe()
        {
            var deltaX = _fingerUpPosition.x - _fingerDownPosition.x;
            var deltaY = _fingerUpPosition.y - _fingerDownPosition.y;

            if (!(Mathf.Abs(deltaX) > _minDistanceForSwipe) && !(Mathf.Abs(deltaY) > _minDistanceForSwipe))
            {
                _direction2D = Direction2D.None;
                return;
            }

            if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            {
                _direction2D = deltaX > 0 ? Direction2D.Right : deltaX < 0 ? Direction2D.Left : Direction2D.None;
            }
            else
            {
                _direction2D = deltaY > 0 ? Direction2D.Up : deltaY < 0 ? Direction2D.Down : Direction2D.None;
            }

            _fingerDownPosition = _fingerUpPosition;
        }

        public void ResetInput()
        {
            _direction2D = Direction2D.None;
            _fingerDownPosition = Vector2.zero;
            _fingerUpPosition = Vector2.zero;
            _isDragging = false;
        }

        public Vector3 SerializeDirection(Direction2D direction2D) => direction2D switch
        {
            Direction2D.Down => Vector3.back,
            Direction2D.Up => Vector3.forward,
            Direction2D.Right => Vector3.right,
            Direction2D.Left => Vector3.left,
            _ => Vector3.zero
        };
    }
}