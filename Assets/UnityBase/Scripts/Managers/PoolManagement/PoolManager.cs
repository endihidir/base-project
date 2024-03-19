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
        
        private IDictionary<Type, PoolableObjectGroup> _poolabeGroups = new Dictionary<Type, PoolableObjectGroup>();

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
            _poolabeGroups.ForEach(x => x.Value?.Dispose());
            _poolManagerSo = null;
            _poolabeGroups = null;
            _objectResolver = null;
        }

        public T GetObject<T>(bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default) where T : IPoolable
        {
            var key = typeof(T);

            if (_poolabeGroups.TryGetValue(key, out var poolableObjectGroup))
            {
                var poolable = poolableObjectGroup.GetObject<T>(show, duration, delay, onComplete);
                return poolable;
            }
            else
            {
                poolableObjectGroup = Create<T>();
                var poolable = poolableObjectGroup.GetObject<T>(show, duration, delay, onComplete);
                return poolable;
            }
        }

        public void HideObject<T>(T poolable, float duration, float delay, Action onComplete = default, bool readLogs = false) where T : IPoolable
        {
            var key = poolable.PoolableObject.GetType();

            if (!_poolabeGroups.TryGetValue(key, out var poolableObject))
            {
                if(readLogs)
                    Debug.LogError($"You can not hide object because {key} is not exist in the list of prefabs.");
                
                return;
            }
            
            poolableObject.HideObject(poolable, duration, delay, onComplete);
        }

        public void HideAllObjectsOfGroup<T>(float duration, float delay, Action onComplete = default, bool readLogs = false) where T : IPoolable
        {
            var key = typeof(T);

            if (!_poolabeGroups.TryGetValue(key, out var poolableObjectGroup))
            {
                if(readLogs)
                    Debug.LogError($"You can not hide all object because {key} is not exist in the list of prefabs.");
                
                return;
            }

            poolableObjectGroup.HideAllObjects<T>(duration, delay, onComplete);
        }
        
        public void HideAllObjectsOfType<T>(float duration, float delay, Action onComplete = default) where T : IPoolable
        {
            foreach (var poolableGroup in _poolabeGroups.Values)
            {
                poolableGroup.HideAllObjects<T>(duration, delay, onComplete);
            }
        }

        public void HideAll(float duration, float delay, Action onComplete = default)
        {
            foreach (var poolableObject in _poolabeGroups)
            {
                poolableObject.Value.HideAllObjects<IPoolable>(duration, delay, onComplete);
            }
        }

        public void RemovePool<T>(bool readLogs = false) where T : IPoolable
        {
            if(_isDisposed) return;
            
            var key = typeof(T);

            if (!_poolabeGroups.TryGetValue(key, out var poolableObjectGroup))
            {
                if(readLogs)
                    Debug.LogError($"You can not remove pool because {key} is not exist in the list of prefabs.");
                
                return;
            }

            poolableObjectGroup.ClearAll<T>();
            
            _poolabeGroups.Remove(key);
        }

        public int GetClonesCount<T>(bool readLogs = false) where T : IPoolable
        {
            var key = typeof(T);

            if (!_poolabeGroups.TryGetValue(key, out var poolableObjectGroup))
            {
                if(readLogs)
                    Debug.LogError($"You can not get pool count because {key} is not exist in the list of prefabs.");
                
                return 0;
            }

            return poolableObjectGroup.Pool.Count;
        }

        public IEnumerable<T> GetActivePoolables<T>() where T : IPoolable
        {
            var key = typeof(T);

            var newList = Enumerable.Empty<T>();

            if (_poolabeGroups.TryGetValue(key, out var poolableObjectGroup))
            {
                newList = poolableObjectGroup.FindActivePoolables<T>();
            }

            return newList;
        }

        public void UpdateAllResolvers(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
            
            foreach (var poolableObject in _poolabeGroups)
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
                if (_poolabeGroups.ContainsKey(key)) continue;
                var poolableObjectGroup = new PoolableObjectGroup();
                poolableObjectGroup.Initialize(poolable, _poolableObjectsParent, poolableAsset.poolSize, poolableAsset.isLazy, _objectResolver);
                _poolabeGroups.Add(key, poolableObjectGroup);
            }
        }
        
        private void CreateAllCashedPrefabs() => _poolabeGroups.Where(poolData=> !poolData.Value.IsLazy)
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
            _poolabeGroups.Add(type, poolableObjectGroup);
            return poolableObjectGroup;
        }
    }
}