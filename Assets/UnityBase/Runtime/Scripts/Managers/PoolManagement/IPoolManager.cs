using System;
using UnityBase.Pool;
using UnityEngine;

namespace UnityBase.Service
{
    public interface IPoolManager
    {
        public T GetObject<T>(T objectRef, bool show = true, int poolCount = 1, Action onComplete = null) where T : Component;
        public T GetObject<T>(bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default) where T : Component, IPoolable;
        public void HideObject<T>(T objectRef, float duration, float delay, Action onComplete = default) where T : Component;
        public void ReturnToPool<T>(T objectRef, Action onComplete = default) where T : IPoolable;

        public void HideAllObjectsOfType<T>(float duration, float delay, Action onComplete = default) where T : Component, IPoolable;
        public void HideAll(float duration, float delay, Action onComplete = default);
        public void RemovePool<T>() where T : IPoolable;
        public int GetPoolCount<T>() where T : IPoolable;
    }
}