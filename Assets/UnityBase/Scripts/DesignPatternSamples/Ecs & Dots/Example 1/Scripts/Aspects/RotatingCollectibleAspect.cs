using Unity.Entities;
using Unity.Transforms;

namespace UnityBase.DesignPatterns.ECS
{
    [WithAll(typeof(RotatingCollectible))]
    public readonly partial struct RotatingCollectibleAspect : IAspect
    {
        public readonly RefRO<RotatingCollectible> rotatingCollectible;
        public readonly RefRW<LocalTransform> localTransform;
        public readonly RefRO<RotateSpeed> rotateSpeed;

        public void Rotate(float deltaTime)
        {
            localTransform.ValueRW = localTransform.ValueRO.RotateY(rotateSpeed.ValueRO.value * deltaTime);
        }
    }
}