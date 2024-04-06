using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
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
        private IObjectResolver _objectResolver;
        private readonly Queue<IPoolable> _pool;
        public Queue<IPoolable> Pool => _pool;
        public bool IsLazy => _isLazy;

        public PoolableObjectGroup() => _pool = new Queue<IPoolable>();

        public void Initialize(IPoolable poolable, Transform rootParent, int poolCount, bool isLazy, IObjectResolver objectResolver)
        {
            _poolable = poolable;
            _poolableRoot = rootParent;
            _poolCount = poolCount;
            _objectResolver = objectResolver;
            _isLazy = isLazy;
            CreatePoolParent();
        }
        
        public void UpdateObjectResolver(IObjectResolver objectResolver) => _objectResolver = objectResolver;

        public void CreatePool()
        {
            for (int i = 0; i < _poolCount; i++) 
                CreateNewObject(true);
        }

        public T GetObject<T>(bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default) where T : IPoolable
        {
            if (!_poolParent) CreatePoolParent();
            
            if (IsAnyPoolableMissing()) ClearPool();
            
            IPoolable poolable;
            
            if (_poolable.IsUnique)
            {
                if (!_pool.TryPeek(out poolable)) 
                    poolable = GetNewPoolable();
            }
            else
            {
                if (!_pool.TryDequeue(out poolable)) 
                    poolable = GetNewPoolable();
            }

            if (show) 
                poolable?.Show(duration, delay, onComplete);

            return (T)poolable;
        }

        public void HideObject<T>(T poolable, float duration, float delay, Action onComplete) where T : IPoolable
        {
            if (!poolable.IsActive) return;

            poolable.Hide(duration, delay, ()=> OnHideComplete(poolable, onComplete));
        }

        private void ClearPool()
        {
            _pool?.Where(poolable => poolable.PoolableObject)
                  .ForEach(poolable => Object.Destroy(poolable.PoolableObject.gameObject));

            _pool?.Clear();

            if (_poolParent) 
                Object.Destroy(_poolParent);
        }

        public void ClearAll<T>() where T : IPoolable
        {
            ClearPool();
            
            FindPoolablesOfType<T>()?.ForEach(poolable => Object.Destroy(poolable.PoolableObject.gameObject));
        }

        private IPoolable GetNewPoolable()
        {
            CreateNewObject(false);
            
            return _pool.Dequeue();
        }

        private void CreateNewObject(bool onInitialize)
        {
            var poolableObject = Object.Instantiate(_poolable.PoolableObject, _poolParent.transform);
            
            _objectResolver.InjectGameObject(poolableObject.gameObject);

            poolableObject.name = _poolable.PoolableObject.name;
            
            var poolable = poolableObject.GetComponent<IPoolable>();

            if (onInitialize)
            {
                poolable.Hide(0f, 0f, default);
            }

            _pool.Enqueue(poolable);
        }

        private void OnHideComplete(IPoolable poolable, Action onComplete)
        {
            if(!_poolParent) CreatePoolParent();
            var poolableT = poolable.PoolableObject.transform;
            poolableT.SetParent(_poolParent.transform);
            poolableT.localPosition = Vector3.zero;
            _pool.Enqueue(poolable);
            onComplete?.Invoke();
        }
        
        public static IEnumerable<T> FindPoolablesOfType<T>(bool inculedInactive = false) where T : IPoolable
        {
            return Object.FindObjectsOfType<MonoBehaviour>(inculedInactive).OfType<T>().Where(poolable => poolable.IsActive);
        }
        
        private bool IsAnyPoolableMissing() => _pool.Any(poolable => !poolable.PoolableObject);

        private void CreatePoolParent()
        {
            _poolParent = new GameObject("Pool_" + _poolable.PoolableObject.name);
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