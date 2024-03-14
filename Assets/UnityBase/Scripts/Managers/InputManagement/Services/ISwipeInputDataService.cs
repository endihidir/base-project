using UnityEngine;

namespace UnityBase.Service
{
    public interface ISwipeInputDataService
    {
        public Direction GetSwipeDirection();
        public Vector3 SerializeDirection(Direction direction);
        public void ResetInput();
    }
}