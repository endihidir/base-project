using UnityBase.Tag;
using UnityEngine;

namespace UnityBase.ManagerSO
{
    [CreateAssetMenu(menuName = "Game/ManagerData/PoolManagerData", fileName = "PoolManagerSO")]
    public class PoolManagerSO : ScriptableObject
    {
        public Transform poolParentTransform;

        [Header("POOL DATA")] public PoolableAssetSO[] poolDataSo;

        public void Initialize()
        {
            poolParentTransform = FindObjectOfType<Tag_PoolableObjectHolder>()?.transform;
        }
    }
}