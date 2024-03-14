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
        private IList<IPoolable> _pool = new List<IPoolable>();
        
        public IList<IPoolable> Pool => _pool;
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

        public IPoolable ShowObject(float duration, float delay, Action onComplete)
        {
            if (!_poolParent) CreatePoolParent();

            var poolable = _pool.FirstOrDefault(IsShowable) ?? CreateNewObject(false);

            poolable?.Show(duration, delay, onComplete);

            return poolable;
        }

        private bool IsShowable(IPoolable poolable) => !poolable.IsActive || poolable.IsUnique;

        public void HideObject(IPoolable poolable, float duration, float delay, Action onComplete)
        {
            if (!IsHidable(poolable)) return;
            
            poolable.Hide(duration, delay, ()=>
            {
                SetPoolableObjectParent(poolable);
                onComplete?.Invoke();
            });
        }

        private bool IsHidable(IPoolable poolable) => poolable.IsActive || poolable.IsUnique;

        public void HideAllObjects(float duration, float delay, Action onComplete)
        {
            _pool.ForEach(poolable => HideObject(poolable, duration, delay, onComplete));
        }

        public void Remove(IPoolable poolable)
        {
            if (!_pool.Contains(poolable)) return;

            if (poolable.PoolableObject) 
                UnityEngine.Object.Destroy(poolable.PoolableObject.gameObject);

            _pool.Remove(poolable);
        }

        public void Clear()
        {
            _pool.Where(poolable => poolable.PoolableObject)
                 .ForEach(poolable => Object.Destroy(poolable.PoolableObject.gameObject));

            _pool?.Clear();

            Object.Destroy(_poolParent);
        }

        public void UpdateObjectResolver(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
        }

        private IPoolable CreateNewObject(bool onInitialize)
        {
            var poolableObject = Object.Instantiate(_poolable.PoolableObject, _poolParent.transform);
            
            _objectResolver.InjectGameObject(poolableObject.gameObject);

            poolableObject.name = _poolable.PoolableObject.name;
            
            var poolable = poolableObject.GetComponent<IPoolable>();

            if (onInitialize)
            {
                poolable.Hide(0f, 0f, default);
            }

            _pool.Add(poolable);

            return poolable;
        }

        private void SetPoolableObjectParent(IPoolable poolable)
        {
            if(!_poolParent) CreatePoolParent();
            
            poolable.PoolableObject.transform.SetParent(_poolParent.transform);
            
            poolable.PoolableObject.transform.localPosition = Vector3.zero;
        }
        
        private void CreatePoolParent()
        {
            _poolParent = new GameObject("Pool_" + _poolable.PoolableObject.name);
            _poolParent.transform.SetParent(_rootParent);
        }

        public void Dispose()
        {
            Clear();
            _poolable = default;
            _rootParent = null;
            _poolParent = null;
            _poolCount = 0;
            _pool?.Clear();
        }
    }
}