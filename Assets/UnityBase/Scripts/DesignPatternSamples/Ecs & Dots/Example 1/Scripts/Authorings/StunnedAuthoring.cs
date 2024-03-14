using Unity.Entities;
using UnityEngine;

namespace UnityBase.DesignPatterns.ECS
{
    public class StunnedAuthoring : MonoBehaviour
    {
        public class Baker : Baker<StunnedAuthoring>
        {
            public override void Bake(StunnedAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Stunned());
                SetComponentEnabled<Stunned>(entity, false);
            }
        }
    }

    public struct Stunned : IComponentData, IEnableableComponent
    {
        
    }
}