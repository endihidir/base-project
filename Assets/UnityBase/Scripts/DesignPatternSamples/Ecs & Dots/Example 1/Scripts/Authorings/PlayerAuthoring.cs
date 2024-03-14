using Unity.Entities;
using UnityEngine;

namespace UnityBase.DesignPatterns.ECS
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Player());
            }
        }
    }

    public struct Player : IComponentData
    {
        
    }
}