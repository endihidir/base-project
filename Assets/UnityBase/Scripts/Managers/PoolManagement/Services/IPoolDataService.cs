using System;
using UnityBase.Pool;

namespace UnityBase.Service
{
    public interface IPoolDataService
    {
        public T GetObject<T>(bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default) where T : IPoolable;
        public void HideObject<T>(T poolable, float duration, float delay, Action onComplete = default, bool readLogs = false) where T : IPoolable;
        public void HideAllObjectsOfType<T>(float duration, float delay, Action onComplete = default) where T : IPoolable;
        public void HideAll(float duration, float delay, Action onComplete = default);
        public void RemovePool<T>(bool readLogs = false) where T : IPoolable;
        public int GetPoolCount<T>(bool readLogs = false) where T : IPoolable;
    }
}