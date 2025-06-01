using System;
using UnityBase.Pool;
using UnityEngine;

namespace UnityBase.Manager
{
    public class PoolableObject : MonoBehaviour, IPoolable
    {
        public bool IsActive => gameObject.activeInHierarchy;
        public bool IsUnique => false;
        public int PoolKey { get; set; }

        public void Show(float duration, float delay, Action onComplete)
        {
            gameObject.SetActive(true);
            onComplete?.Invoke();
        }

        public void Hide(float duration, float delay, Action onComplete)
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }
    }
}