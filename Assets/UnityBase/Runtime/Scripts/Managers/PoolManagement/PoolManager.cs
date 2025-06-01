using System;
using System.Collections.Generic;
using System.Linq;
using UnityBase.BootService;
using UnityBase.GameDataHolder;
using UnityBase.Managers.SO;
using UnityBase.Pool;
using UnityBase.Service;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityBase.Manager
{
    public class PoolManager : IPoolManager, IAppBootService
    {
        private readonly PoolManagerSO _poolManagerSo;
        
        private Transform _poolableObjectsParent;
        
        private IDictionary<int, PoolableObjectGroup> _poolableObjectGroupsWithID = new Dictionary<int, PoolableObjectGroup>();
        
        private IDictionary<Type, PoolableObjectGroup> _poolableGroupsWithType = new Dictionary<Type, PoolableObjectGroup>();

        private bool _isDisposed;
        
        public PoolManager(GameDataHolderSO gameDataHolderSo)
        {
            _poolManagerSo = gameDataHolderSo.poolManagerSo;
            
            _poolableObjectsParent = _poolManagerSo.poolParentTransform;
            
            _isDisposed = false;
        }

        ~PoolManager() => Dispose();

        public void Initialize()
        {
            CacheAllPoolableObjects();
            
            CreateAllCachedPoolableObjects();
        }

        public void Dispose()
        {
            if(_isDisposed) return;
            
            _isDisposed = true;
            
            foreach (var poolableObjectGroup in _poolableGroupsWithType)
            {
                poolableObjectGroup.Value?.Dispose();
            }
           
            _poolableGroupsWithType = null;
        }
        
        public T GetObject<T>(T objectRef, bool show = true, int poolCount = 1, Action onComplete = null) where T : Component
        {
            var key = objectRef.gameObject.GetHashCode();
            
            if (_poolableObjectGroupsWithID.TryGetValue(key, out var poolableObjectGroup))
            {
                var poolable = poolableObjectGroup.GetObject<T>(show, 0f, 0f, onComplete);
                
                return poolable;
            }
            else
            {
                poolableObjectGroup = CreateNewGroup(objectRef, poolCount);
                
                var poolable = poolableObjectGroup.GetObject<T>(show, 0f, 0f, onComplete);

                return poolable;
            }
        }

        public T GetObject<T>(bool show = true, float duration = 0f, float delay = 0f, Action onComplete = null) where T : Component, IPoolable
        {
            var key = typeof(T);

            if (_poolableGroupsWithType.TryGetValue(key, out var poolableObjectGroup))
            {
                var poolable = poolableObjectGroup.GetObject<T>(show, duration, delay, onComplete);
                
                return poolable;
            }
            else
            {
                poolableObjectGroup = CreateNewGroup<T>();
                
                var poolable = poolableObjectGroup.GetObject<T>(show, duration, delay, onComplete);

                return poolable;
            }
        }

        public void HideObject<T>(T objectRef, float duration, float delay, Action onComplete = null) where T : Component
        {
            if (_isDisposed) return;

            var typeFound = _poolableGroupsWithType.TryGetValue(typeof(T), out var poolableTypeGroup);
            var idFound = false;
            PoolableObjectGroup poolableIdGroup = null;

            if (!typeFound && objectRef.TryGetComponent<IPoolable>(out var poolable))
            {
                idFound = _poolableObjectGroupsWithID.TryGetValue(poolable.PoolKey, out poolableIdGroup);
            }

            if (!typeFound && !idFound)
            {
                Debug.LogError($"[PoolManager] HideObject failed: Neither Type '{typeof(T).Name}' nor Object '{objectRef.name}' (Hash: {objectRef.GetHashCode()}) exists in the pool.");
                return;
            }

            var poolableObjectGroup = typeFound ? poolableTypeGroup : poolableIdGroup;
            poolableObjectGroup?.HideObject(objectRef, duration, delay, onComplete);
        }
        
        public void ReturnToPool<T>(T objectRef, Action onComplete = null) where T : IPoolable
        {
            if(_isDisposed) return;

            if (!_poolableGroupsWithType.TryGetValue(typeof(T), out var poolableObjectGroup))
            {
                Debug.LogError($"You can not hide object because {typeof(T)} does not exist in the list of prefabs.");
            }
            
            poolableObjectGroup?.ReturnToPool(objectRef, onComplete);
        }
        
        public void HideAllObjectsOfType<T>(float duration, float delay, Action onComplete = null) where T : Component, IPoolable
        {
            if(_isDisposed) return;
            
            var poolables = PoolableObjectGroup.FindPoolableObjectsOfType<T>();
            
            foreach (var poolable in poolables)
            {
                HideObject(poolable, duration, delay, onComplete);
            }
        }

        public void HideAll(float duration, float delay, Action onComplete = null)
        {
            if(_isDisposed) return;
            
            var poolables = PoolableObjectGroup.FindPoolableObjectsOfType<IPoolable>();
            
            foreach (var poolable in poolables)
            {
                if (poolable is Component poolableComponent)
                {
                    HideObject(poolableComponent, duration, delay, onComplete);
                }
            }
        }

        public void RemovePool<T>() where T : IPoolable
        {
            if(_isDisposed) return;
            
            var key = typeof(T);

            if (!_poolableGroupsWithType.TryGetValue(key, out var poolableObjectGroup))
            {
                Debug.LogError($"You can not remove pool because {key} does not exist in the list of prefabs.");
                return;
            }

            poolableObjectGroup.ClearAll<T>();
            
            _poolableGroupsWithType.Remove(key);
        }

        public int GetPoolCount<T>() where T : IPoolable
        {
            var key = typeof(T);

            if (!_poolableGroupsWithType.TryGetValue(key, out var poolableObjectGroup))
            {
                Debug.LogError($"You can not get pool count because {key} does not exist in the list of prefabs.");
                return 0;
            }

            return poolableObjectGroup.Pool.Count;
        }

        private void CacheAllPoolableObjects()
        {
            var poolData = _poolManagerSo.poolDataSo;

            foreach (var poolableAsset in poolData.Distinct())
            {
                if (!poolableAsset.poolObject)
                {
                    Debug.LogError("There is missing prefab in pool object list!");
                    continue;
                }

                var isPoolable = poolableAsset.poolObject.TryGetComponent<IPoolable>(out var poolable);
                
                if(!isPoolable) continue;
                
                var key = poolable.GetType();
                
                if (_poolableGroupsWithType.ContainsKey(key)) continue;
                
                var poolableObjectGroup = new PoolableObjectGroup();
                
                poolableObjectGroup.Initialize(poolableAsset.poolObject, _poolableObjectsParent, poolableAsset.poolSize, poolableAsset.isLazy);
                
                _poolableGroupsWithType.Add(key, poolableObjectGroup);
            }
        }
        
        private void CreateAllCachedPoolableObjects()
        {
            var nonLazyPoolables = _poolableGroupsWithType.Where(poolData => !poolData.Value.IsLazy).ToDictionary(x=> x.Key,y => y.Value);

            foreach (var poolableObjectGroup in nonLazyPoolables)
            {
               poolableObjectGroup.Value?.CreatePool(); 
            }
        }

        private PoolableObjectGroup CreateNewGroup<T>(T objectRef = default, int poolCount = 1)
        {
            var poolableObjectGroup = new PoolableObjectGroup();
            
            if (objectRef is Component poolableComponent)
            {
                var poolKey = poolableComponent.gameObject.GetHashCode();

                poolableObjectGroup.Initialize(poolableComponent.gameObject, _poolableObjectsParent, poolCount)
                                   .SetPoolKey(poolKey)
                                   .CreatePool();
                        
                _poolableObjectGroupsWithID.Add(poolKey, poolableObjectGroup);
            }
            else
            {
                var type = typeof(T);
            
                var poolData = _poolManagerSo.poolDataSo;
            
                var poolableAsset = poolData.FirstOrDefault(x => x.poolObject.GetComponent<IPoolable>() is T);
            
                if (!poolableAsset) return null;
            
                poolableObjectGroup.Initialize(poolableAsset.poolObject, _poolableObjectsParent, poolableAsset.poolSize, poolableAsset.isLazy)
                                   .CreatePool();
                
                _poolableGroupsWithType.Add(type, poolableObjectGroup);
            }
            
            return poolableObjectGroup;
        }
    }
}