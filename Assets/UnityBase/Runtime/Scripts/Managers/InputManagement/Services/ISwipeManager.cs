using UnityEngine;

namespace UnityBase.Service
{
    public interface ISwipeManager
    {
        public Direction2D GetSwipeDirection();
        public Vector3 SerializeDirection(Direction2D direction2D);
        public void ResetInput();
    }
}