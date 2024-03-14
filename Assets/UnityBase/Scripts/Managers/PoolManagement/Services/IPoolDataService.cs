using System;
using System.Collections.Generic;
using UnityBase.Pool;
using UnityEngine;

namespace UnityBase.Service
{
    public interface IPoolDataService
    {
        public T GetObject<T>(float duration, float delay, Action onComplete = default) where T : IPoolable;
        public void HideObject<T>(T poolable, float duration, float delay, Action onComplete = default, bool readLogs = false) where T : IPoolable;
        public void HideAllObjectsOfType<T>(float duration, float delay, Action onComplete = default, bool readLogs = false) where T : IPoolable;
        public void HideAllTypeOf<T>(float duration, float delay, Action onComplete = default) where T :  IPoolable;
        public void HideAll(float duration, float delay, Action onComplete = default);
        public void Remove<T>(T poolable, bool readLogs = false) where T : IPoolable;
        public void RemovePool<T>(bool readLogs = false) where T : IPoolable;
        public int GetClonesCount<T>(bool readLogs = false) where T : IPoolable;
        public List<T> GetClones<T>() where T : Component, IPoolable;
    }
}