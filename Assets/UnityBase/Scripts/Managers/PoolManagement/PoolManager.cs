using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityBase.ManagerSO;
using UnityBase.Pool;
using UnityBase.Service;
using UnityEngine;
using VContainer;

namespace UnityBase.Manager
{
    public class PoolManager : IPoolDataService, IAppPresenterDataService
    {
        private PoolManagerSO _poolManagerSo;
        
        private Transform _poolableObjectsParent;
        
        private IObjectResolver _objectResolver;
        
        private IDictionary<Type, PoolableObjectGroup> _cachedPools = new Dictionary<Type, PoolableObjectGroup>();

        private bool _isDisposed;
        
        public PoolManager(ManagerDataHolderSO managerDataHolderSo, IObjectResolver objectResolver)
        {
            _poolManagerSo = managerDataHolderSo.poolManagerSo;
            _poolableObjectsParent = _poolManagerSo.poolParentTransform;
            _objectResolver = objectResolver;
            _isDisposed = false;
        }

        ~PoolManager() => Dispose();
        public void Initialize() => CashPrefabs();
        public void Start() => CreateAllCashedPrefabs();
        
        public void Dispose()
        {
            if(_isDisposed) return;
            _isDisposed = true;
            _cachedPools.ForEach(x => x.Value?.Dispose());
            _poolManagerSo = null;
            _cachedPools = null;
            _objectResolver = null;
        }

        public T GetObject<T>(float duration, float delay, Action onComplete = default) where T : IPoolable
        {
            var key = typeof(T);

            if (_cachedPools.TryGetValue(key, out var poolableObjectGroup))
            {
                var poolable = poolableObjectGroup.ShowObject(duration, delay, onComplete);
                return (T)poolable;
            }
            else
            {
                poolableObjectGroup = Create<T>();
                var poolable = poolableObjectGroup.ShowObject(duration, delay, onComplete);
                return (T)poolable;
            }
        }

        public void HideObject<T>(T poolable, float duration, float delay, Action onComplete = default, bool readLogs = false) where T : IPoolable
        {
            var key = poolable.PoolableObject.GetType();

            if (!_cachedPools.TryGetValue(key, out var poolableObject))
            {
                if(readLogs)
                    Debug.LogError($"You can not hide object because {key} is not exist in the list of prefabs.");
                
                return;
            }
            
            poolableObject.HideObject(poolable, duration, delay, onComplete);
        }

        public void HideAllObjectsOfType<T>(float duration, float delay, Action onComplete = default, bool readLogs = false) where T : IPoolable
        {
            var key = typeof(T);

            if (!_cachedPools.TryGetValue(key, out var poolableObjectGroup))
            {
                if(readLogs)
                    Debug.LogError($"You can not hide all object because {key} is not exist in the list of prefabs.");
                
                return;
            }

            poolableObjectGroup.HideAllObjects(duration, delay, onComplete);
        }
        
        public void HideAllTypeOf<T>(float duration, float delay, Action onComplete = default) where T : IPoolable
        {
            foreach (var poolableObject in _cachedPools)
            {
                if (poolableObject.Value.GetType() == typeof(T))
                {
                    poolableObject.Value.HideAllObjects(duration, delay, onComplete);
                }
            }
        }

        public void HideAll(float duration, float delay, Action onComplete = default)
        {
            foreach (var poolableObject in _cachedPools)
            {
                poolableObject.Value.HideAllObjects(duration, delay, onComplete);
            }
        }
        
        public void Remove<T>(T poolable, bool readLogs = false) where T : IPoolable
        {
            if(_isDisposed) return;
            
            var key = poolable.PoolableObject.GetType();

            if (!_cachedPools.TryGetValue(key, out var poolableObjectGroup))
            {
                if(readLogs)
                    Debug.LogError($"You can not remove object because {key} is not exist in the list of prefabs.");
                
                return;
            }

            poolableObjectGroup.Remove(poolable);
        }

        public void RemovePool<T>(bool readLogs = false) where T : IPoolable
        {
            if(_isDisposed) return;
            
            var key = typeof(T);

            if (!_cachedPools.TryGetValue(key, out var poolableObjectGroup))
            {
                if(readLogs)
                    Debug.LogError($"You can not remove pool because {key} is not exist in the list of prefabs.");
                
                return;
            }

            poolableObjectGroup.Clear();
            
            _cachedPools.Remove(key);
        }

        public int GetClonesCount<T>(bool readLogs = false) where T : IPoolable
        {
            var key = typeof(T);

            if (!_cachedPools.TryGetValue(key, out var poolableObjectGroup))
            {
                if(readLogs)
                    Debug.LogError($"You can not get pool count because {key} is not exist in the list of prefabs.");
                
                return 0;
            }
                
            return poolableObjectGroup.Pool.Count;
        }

        public List<T> GetClones<T>() where T : Component, IPoolable
        {
            var key = typeof(T);
            
            var newList = new List<T>();

            if (_cachedPools.TryGetValue(key, out var poolableObjectGroup))
                newList.AddRange(poolableObjectGroup.Pool.Select(poolable => (T)poolable.PoolableObject));

            return newList;
        }

        public void UpdateAllResolvers(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
            
            foreach (var poolableObject in _cachedPools)
            {
                poolableObject.Value.UpdateObjectResolver(_objectResolver);
            }
        }

        private void CashPrefabs()
        {
            var poolData = _poolManagerSo.poolDataSo;

            foreach (var poolableAsset in poolData.Distinct())
            {
                if (!poolableAsset.poolObject)
                {
                    Debug.LogError("There is missing prefab in pool object list!");
                    continue;
                }

                var poolable = poolableAsset.poolObject.GetComponent<IPoolable>();
                var key = poolable.PoolableObject.GetType();
                if (_cachedPools.ContainsKey(key)) continue;
                var poolableObjectGroup = new PoolableObjectGroup();
                poolableObjectGroup.Initialize(poolable, _poolableObjectsParent, poolableAsset.poolSize, poolableAsset.isLazy, _objectResolver);
                _cachedPools.Add(key, poolableObjectGroup);
            }
        }
        
        private void CreateAllCashedPrefabs() => _cachedPools.Where(poolData=> !poolData.Value.IsLazy)
                                                             .ForEach(x => x.Value.CreatePool());

        private PoolableObjectGroup Create<T>() where T : IPoolable
        {
            var type = typeof(T);
            var poolData = _poolManagerSo.poolDataSo;
            var poolableAsset = poolData.FirstOrDefault(x => x.poolObject.GetComponent<IPoolable>().PoolableObject.GetType() == type);
            if (poolableAsset == null) return default;
            var poolableObjectGroup = new PoolableObjectGroup();
            var poolableObject = poolableAsset.poolObject.GetComponent<T>();
            poolableObjectGroup.Initialize(poolableObject, _poolableObjectsParent, poolableAsset.poolSize, poolableAsset.isLazy, _objectResolver);
            poolableObjectGroup.CreatePool();
            _cachedPools.Add(type, poolableObjectGroup);
            return poolableObjectGroup;
        }
    }
}