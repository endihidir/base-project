using System;
using System.Collections.Generic;
using System.Linq;
using UnityBase.GameDataHolder;
using UnityBase.Managers.SO;
using UnityBase.Pool;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class PoolManager : IPoolManager
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

        public void ReturnToPool<T>(T objectRef, float duration, float delay, Action onComplete = null) where T : Component
        {
            if (_isDisposed) return;
            
            if (!objectRef) { DebugLogger.LogError("[PoolManager] Return failed: null/destroyed object"); return; }
            
            PoolableObjectGroup group = null;
            
            if (objectRef.TryGetComponent<IPoolable>(out var poolable))
                _poolableObjectGroupsWithID.TryGetValue(poolable.PoolKey, out group);
            
            if (group == null)
                _poolableGroupsWithType.TryGetValue(objectRef.GetType(), out group);

            if (group == null)
            {
                DebugLogger.LogError($"[PoolManager] Return failed: no group for '{objectRef.name}' ({objectRef.GetType().Name}).");
                return;
            }

            if (duration > 0f || delay > 0f)
            {
                group.HideObject(objectRef, duration, delay, onComplete);
            }
            else
            {
                if (!objectRef.TryGetComponent<IPoolable>(out var p))
                {
                    DebugLogger.LogError("[PoolManager] Return failed: object is not IPoolable.");
                    return;
                }
                
                group.ReturnToPool(p, onComplete);
            }
        }
        
        public void HideAllObjectsOfType<T>(float duration, float delay, Action onComplete = null) where T : Component, IPoolable
        {
            if(_isDisposed) return;
            
            var poolables = PoolableObjectGroup.FindPoolableObjectsOfType<T>();
            
            foreach (var poolable in poolables)
            {
                ReturnToPool(poolable, duration, delay, onComplete);
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
                    ReturnToPool(poolableComponent, duration, delay, onComplete);
                }
            }
        }

        public void RemovePool<T>() where T : IPoolable
        {
            if(_isDisposed) return;
            
            var key = typeof(T);

            if (!_poolableGroupsWithType.TryGetValue(key, out var poolableObjectGroup))
            {
                DebugLogger.LogError($"You can not remove pool because {key} does not exist in the list of prefabs.");
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
                DebugLogger.LogError($"You can not get pool count because {key} does not exist in the list of prefabs.");
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
                    DebugLogger.LogError("There is missing prefab in pool object list!");
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