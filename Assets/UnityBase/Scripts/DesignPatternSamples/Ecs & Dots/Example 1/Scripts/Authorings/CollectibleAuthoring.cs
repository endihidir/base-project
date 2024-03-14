using Unity.Entities;
using UnityEngine;

namespace UnityBase.DesignPatterns.ECS
{
    public class CollectibleAuthoring : MonoBehaviour
    {
        public class Baker : Baker<CollectibleAuthoring>
        {
            public override void Bake(CollectibleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RotatingCollectible());
            }
        }
    }

    public struct RotatingCollectible : IComponentData
    {
        
    }
}