using UnityEngine;

namespace UnityBase.Service
{
    public interface ISwipeManagementService
    {
        public Direction GetSwipeDirection();
        public Vector3 SerializeDirection(Direction direction);
        public void ResetInput();
    }
}