using System;

namespace UnityBase.Pool
{
    public interface IPoolable
    { 
        public bool IsActive { get; }
        public bool IsUnique { get; }
        public int PoolKey { get; set; }
        public void Show(float duration = 0f, float delay = 0f, Action onComplete = null);
        public void Hide(float duration = 0f, float dela = 0f, Action onComplete = null);
    }
}