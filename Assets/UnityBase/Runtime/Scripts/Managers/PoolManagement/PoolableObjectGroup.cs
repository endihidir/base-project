using System;
using System.Collections.Generic;
using System.Linq;
using UnityBase.Manager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityBase.Pool
{
    public sealed class PoolableObjectGroup
    {
        private GameObject _prefab;

        private IPoolable _poolable;
        
        private Transform _poolableRoot;
        
        private int _poolCount;
        
        private bool _isLazy;
        
        private GameObject _poolParent;

        private int _poolKey;
        
        private readonly Queue<IPoolable> _pool = new();
        public Queue<IPoolable> Pool => _pool;
        public bool IsLazy => _isLazy;

        public PoolableObjectGroup Initialize(GameObject prefab, Transform rootParent, int poolCount, bool isLazy = true)
        {
            _prefab = prefab;
            
            _poolableRoot = rootParent;
            
            _poolCount = poolCount;
            
            _isLazy = isLazy;
            
            CreatePoolParent();

            return this;
        }

        public PoolableObjectGroup CreatePool()
        {
            for (int i = 0; i < _poolCount; i++)
            {
                CreateNewObject(true);
            }

            return this;
        }
        
        public PoolableObjectGroup SetPoolKey(int poolKey)
        {
            _poolKey = poolKey;

            return this;
        }

        public T GetObject<T>(bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default) where T : Component
        {
            if (IsAnyPoolableMissing()) ClearPool();
            
            if (!_poolParent) CreatePoolParent();
            
            IPoolable poolable;
            
            T component = null;
            
            if (_poolable.IsUnique)
            {
                if (!_pool.TryPeek(out poolable))
                {
                    poolable = GetNewPoolable();
                }
            }
            else
            {
                if (!_pool.TryDequeue(out poolable))
                {
                    poolable = GetNewPoolable();
                }
            }
            
            if (show) 
                poolable?.Show(duration, delay, onComplete);


            if (poolable is T tComponent)
            {
                component = tComponent;
            }
            else if (poolable is Component comp)
            {
                component = comp.GetComponent<T>();
                Debug.Assert(component, $"[PoolManager] Expected component '{typeof(T).Name}' not found on pooled object '{comp.gameObject.name}'.");
            }
            else
            {
                Debug.Assert(false, $"[PoolManager] IPoolable is not a Component! Object: {poolable}");
            }
            
            
            return component;
        }

        public void HideObject<T>(T poolable, float duration, float delay, Action onComplete) where T : Component
        {
            if (!poolable.TryGetComponent<IPoolable>(out var poolableObj)) return;
            
            if (!poolableObj.IsActive) return;

            poolableObj.Hide(duration, delay, ()=> ReturnToPool(poolableObj, onComplete));
        }

        private void ClearPool()
        {
            foreach (var poolable in _pool)
            {
                if (poolable is Component poolableObj)
                {
                    Object.Destroy(poolableObj);
                }
            }

            _pool?.Clear();

            if (!_poolParent) return;
                
            Object.Destroy(_poolParent);
        }

        public void ClearAll<T>() where T : IPoolable
        {
            ClearPool();

            foreach (var poolable in FindPoolableObjectsOfType<T>())
            {
                if (poolable is Component poolableObj)
                {
                    Object.Destroy(poolableObj);
                }
                else
                {
                    Debug.LogError($"{poolable.GetType()} is not Component!");
                }
            }
        }

        private IPoolable GetNewPoolable()
        {
            CreateNewObject(false);
            
            return _pool.Dequeue();
        }

        private void CreateNewObject(bool onInitialize)
        {
            var poolableObject = Object.Instantiate(_prefab, _poolParent.transform);

            poolableObject.name = _prefab.name;

            if (!poolableObject.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable = poolableObject.AddComponent<PoolableObject>();
            }
            
            poolable.PoolKey = _poolKey;
            
            if (onInitialize)
            {
                _poolable ??= poolable;
                
                poolable.Hide(0f, 0f, default);
            }

            _pool.Enqueue(poolable);
        }

        public void ReturnToPool(IPoolable poolable, Action onComplete)
        {
            if(!_poolParent) CreatePoolParent();

            if (poolable is not Component poolableObj)
            {
                Debug.LogError($"{poolable.GetType()} is not Component!");
                return;
            }
            
            var poolableT = poolableObj.transform;

            poolableT.SetParent(_poolParent.transform);

            poolableT.localPosition = Vector3.zero;

            _pool.Enqueue(poolable);

            onComplete?.Invoke();
        }
        
        public static IEnumerable<T> FindPoolableObjectsOfType<T>(bool inculedInactive = false) where T : IPoolable
        {
            return Object.FindObjectsOfType<MonoBehaviour>(inculedInactive).OfType<T>().Where(poolable => poolable.IsActive);
        }
        
        private bool IsAnyPoolableMissing() => _pool.Any(poolable => poolable is null);

        private void CreatePoolParent()
        {
            _poolParent = new GameObject("Pool_" + _prefab.name);
                
            _poolParent.transform.SetParent(_poolableRoot);
        }

        public void Dispose()
        {
            ClearPool();
            
            _prefab = null;
            
            _poolableRoot = null;
            
            _poolParent = null;
            
            _poolCount = 0;
        }
    }
}