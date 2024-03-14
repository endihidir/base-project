using UnityBase.Service;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityBase.Manager
{
    public class SwipeInputManager : ISwipeInputDataService, IGameplayPresenterDataService
    {
        private float _minDistanceForSwipe = Screen.width * 0.1f;

        private Vector2 _fingerDownPosition, _fingerUpPosition;
        private Direction _direction;

        private bool _isDragging;

        public void Initialize()
        {
        }

        public void Start()
        {
        }

        public void Dispose()
        {
        }

        public Direction GetSwipeDirection()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                ResetInput();
                return _direction;
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

            return _direction;
        }

        private void CheckSwipe()
        {
            var deltaX = _fingerUpPosition.x - _fingerDownPosition.x;
            var deltaY = _fingerUpPosition.y - _fingerDownPosition.y;

            if (!(Mathf.Abs(deltaX) > _minDistanceForSwipe) && !(Mathf.Abs(deltaY) > _minDistanceForSwipe))
            {
                _direction = Direction.None;
                return;
            }

            if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
            {
                _direction = deltaX > 0 ? Direction.Right : deltaX < 0 ? Direction.Left : Direction.None;
            }
            else
            {
                _direction = deltaY > 0 ? Direction.Up : deltaY < 0 ? Direction.Down : Direction.None;
            }

            _fingerDownPosition = _fingerUpPosition;
        }

        public void ResetInput()
        {
            _direction = Direction.None;
            _fingerDownPosition = Vector2.zero;
            _fingerUpPosition = Vector2.zero;
            _isDragging = false;
        }

        public Vector3 SerializeDirection(Direction direction) => direction switch
        {
            Direction.Down => Vector3.back,
            Direction.Up => Vector3.forward,
            Direction.Right => Vector3.right,
            Direction.Left => Vector3.left,
            _ => Vector3.zero
        };
    }
}

public enum Direction
{
    None,
    Up,
    Down,
    Right,
    Left
}