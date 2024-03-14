using System;
using Unity.Entities;
using UnityEngine;

namespace UnityBase.DesignPatterns.ECS
{
    public class MovementAuthoring : MonoBehaviour
    {
        public Movement movement;

        public class Baker : Baker<MovementAuthoring>
        {
            public override void Bake(MovementAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, authoring.movement);
            }
        }
    }
    
    [Serializable]
    public struct Movement : IComponentData
    {
        public float speed;
    }
}