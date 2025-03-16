using System;
using System.Collections.Generic;
using System.Linq;
using UnityBase.Manager;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace UnityBase.Pool
{
    public sealed class PoolableObjectGroup
    {
        private IPoolable _poolable;
        
        private Transform _poolableRoot;
        
        private int _poolCount;
        
        private bool _isLazy;
        
        private GameObject _poolParent;
        
        private IObjectResolverContainer _objectResolverContainer;
        
        private readonly Queue<IPoolable> _pool;
        public Queue<IPoolable> Pool => _pool;
        public bool IsLazy => _isLazy;

        public PoolableObjectGroup() => _pool = new Queue<IPoolable>();
        
      
        public void Initialize(IObjectResolverContainer objectResolverContainer, IPoolable poolable, Transform rootParent, int poolCount, bool isLazy)
        {
            _objectResolverContainer = objectResolverContainer;
            
            _poolable = poolable;
            
            _poolableRoot = rootParent;
            
            _poolCount = poolCount;
            
            _isLazy = isLazy;
            
            CreatePoolParent();
        }

        public void CreatePool()
        {
            for (int i = 0; i < _poolCount; i++)
            {
                CreateNewObject(true);
            }
        }

        public T GetObject<T>(bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default) where T : IPoolable
        {
            if (IsAnyPoolableMissing()) ClearPool();
            
            if (!_poolParent) CreatePoolParent();
            
            IPoolable poolable;
            
            if (_poolable.IsUnique)
            {
                if (_pool.TryPeek(out poolable) && poolable is Component poolableObj)
                {
                    _objectResolverContainer.ObjectResolver.InjectGameObject(poolableObj.gameObject);
                }
                else
                {
                    poolable = GetNewPoolable();
                }
            }
            else
            {
                if (_pool.TryDequeue(out poolable) && poolable is Component poolableObj)
                {
                    _objectResolverContainer.ObjectResolver.InjectGameObject(poolableObj.gameObject);
                }
                else
                {
                    poolable = GetNewPoolable();
                }
            }

            if (show) 
                poolable?.Show(duration, delay, onComplete);

            return (T)poolable;
        }

        public void HideObject<T>(T poolable, float duration, float delay, Action onComplete) where T : IPoolable
        {
            if (!poolable.IsActive) return;

            poolable.Hide(duration, delay, ()=> ReturnToPool(poolable, onComplete));
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

            foreach (var poolable in FindPoolablesOfType<T>())
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
            if (_poolable is not Component poolableComponent) return;
            
            var poolableObject = Object.Instantiate(poolableComponent, _poolParent.transform);
                
            _objectResolverContainer.ObjectResolver.InjectGameObject(poolableObject.gameObject);

            poolableObject.name = poolableComponent.name;

            if (poolableObject is not IPoolable poolable) return;
            
            if (onInitialize)
            {
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
        
        public static IEnumerable<T> FindPoolablesOfType<T>(bool inculedInactive = false) where T : IPoolable
        {
            return Object.FindObjectsOfType<MonoBehaviour>(inculedInactive).OfType<T>().Where(poolable => poolable.IsActive);
        }
        
        private bool IsAnyPoolableMissing() => _pool.Any(poolable => poolable is null);

        private void CreatePoolParent()
        {
            if (_poolable is not Component poolableObj)
            {
                Debug.LogError($"{_poolable?.GetType()} is not Component!");
                return;
            }
            
            _poolParent = new GameObject("Pool_" + poolableObj.name);
                
            _poolParent.transform.SetParent(_poolableRoot);
        }

        public void Dispose()
        {
            ClearPool();
            
            _poolable = default;
            
            _poolableRoot = null;
            
            _poolParent = null;
            
            _poolCount = 0;
        }
    }
}