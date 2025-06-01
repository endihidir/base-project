using System;

namespace UnityBase.Pool
{
    public interface IPoolable
    { 
        public bool IsActive { get; }
        public bool IsUnique { get; }
        public int PoolKey { get; set; }
        public void Show(float duration, float delay, Action onComplete);
        public void Hide(float duration, float delay, Action onComplete);
    }
}