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
        private Transform _rootParent;
        private int _poolCount;
        private bool _isLazy;
        
        private GameObject _poolParent;
        private IObjectResolver _objectResolver;
        private Queue<IPoolable> _pool = new Queue<IPoolable>();

        public Queue<IPoolable> Pool => _pool;
        public bool IsLazy => _isLazy;

        public void Initialize(IPoolable poolable, Transform rootParent, int poolCount, bool isLazy, IObjectResolver objectResolver)
        {
            _poolable = poolable;
            _rootParent = rootParent;
            _poolCount = poolCount;
            _objectResolver = objectResolver;
            _isLazy = isLazy;
            CreatePoolParent();
        }

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

        public void HideObject(IPoolable poolable, float duration, float delay, Action onComplete)
        {
            if (!IsHidable(poolable)) return;

            poolable.Hide(duration, delay, ()=> SetPoolableObjectParent(poolable, onComplete));
        }

        public void HideAllObjects<T>(float duration, float delay, Action onComplete) where T : IPoolable
        {
            var allPoolables = FindActivePoolables<T>();
            
            allPoolables?.ForEach(poolable => HideObject(poolable, duration, delay, onComplete));
        }

        private void ClearPool()
        {
            _pool?.Where(poolable => poolable.PoolableObject)
                  .ForEach(poolable => Object.Destroy(poolable.PoolableObject.gameObject));

            _pool?.Clear();

            if (_poolParent)
            {
                Object.Destroy(_poolParent);
            }
        }

        public void ClearAll<T>() where T : IPoolable
        {
            ClearPool();
            
            FindActivePoolables<T>().ForEach(poolable => Object.Destroy(poolable.PoolableObject.gameObject));
        }

        public void UpdateObjectResolver(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
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

        private void SetPoolableObjectParent(IPoolable poolable, Action onComplete)
        {
            if(!_poolParent) CreatePoolParent();
            var poolableT = poolable.PoolableObject.transform;
            poolableT.SetParent(_poolParent.transform);
            poolableT.localPosition = Vector3.zero;
            _pool.Enqueue(poolable);
            onComplete?.Invoke();
        }
        
        public IEnumerable<T> FindActivePoolables<T>() where T : IPoolable
        {
            return Object.FindObjectsOfType<MonoBehaviour>().OfType<T>().Where(IsHidable);
        }
        
        private bool IsHidable<T>(T poolable) where T : IPoolable => poolable.IsActive || poolable.IsUnique;
        private bool IsAnyPoolableMissing() => _pool.Any(poolable => !poolable.PoolableObject);

        private void CreatePoolParent()
        {
            _poolParent = new GameObject("Pool_" + _poolable.PoolableObject.name);
            _poolParent.transform.SetParent(_rootParent);
        }

        public void Dispose()
        {
            ClearPool();
            _poolable = default;
            _rootParent = null;
            _poolParent = null;
            _poolCount = 0;
            _pool?.Clear();
        }
    }
}