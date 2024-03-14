using Unity.Entities;
using UnityEngine;

namespace UnityBase.DesignPatterns.ECS
{
    public class CollectibleSpawnAuthoring : MonoBehaviour
    {
        public GameObject collectiblePrefab;
        public int amountToSpawn;
        
        public class Baker : Baker<CollectibleSpawnAuthoring>
        {
            public override void Bake(CollectibleSpawnAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                
                AddComponent(entity, new CollectibleSpawn
                {
                    cubePrefabEntity = GetEntity(authoring.collectiblePrefab, TransformUsageFlags.Dynamic), 
                    amountToSpawn = authoring.amountToSpawn
                });
            }
        }
    }

    public struct CollectibleSpawn : IComponentData
    {
        public Entity cubePrefabEntity;
        public int amountToSpawn;
    }
}