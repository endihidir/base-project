using Unity.Entities;
using UnityEngine;

namespace UnityBase.DesignPatterns.ECS
{
    public class RotateAuthoring : MonoBehaviour
    {
        public float value;
        
        private class Baker : Baker<RotateAuthoring>
        {
            public override void Bake(RotateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RotateSpeed{value = authoring.value});
            }
        }
    }
    
    public struct RotateSpeed : IComponentData
    {
        public float value;
    }
}