using Sirenix.OdinInspector;
using UnityBase.Pool;
using UnityEngine;

namespace UnityBase.ManagerSO
{
    [CreateAssetMenu(menuName = "Game/PoolManagement/PoolableAsset", fileName = "PoolableAsset")]
    public class PoolableAssetSO : ScriptableObject
    {
        [HideIf("_isUnique")] 
        public int poolSize;
        
        [Required]
        public GameObject poolObject;
        
        public bool isLazy;
        
        [System.NonSerialized]
        private bool _isUnique;
        
        private void OnEnable()
        {
            if(!poolObject) return;
            
            var poolable = poolObject.GetComponentInChildren<IPoolable>(true);

            _isUnique = poolable?.IsUnique ?? false;

            poolSize = _isUnique ? 1 : poolSize;
        }
    }
}
