using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace UnityBase.DesignPatterns.ECS
{
    public partial struct RotatingCollectibleSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RotateSpeed>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // First implementation option
            
            /*foreach (var (localTransform,rotateSpeed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotateSpeed>>().WithAll<RotatingCollectible>())
            {
                localTransform.ValueRW = localTransform.ValueRO.RotateY(rotateSpeed.ValueRO.value * SystemAPI.Time.DeltaTime);
            }*/
            
            // Second implementation option
            
            /*foreach (var rotatingCollectibleAspect in SystemAPI.Query<RotatingCollectibleAspect>().WithAll<RotatingCollectible>())
            {
                rotatingCollectibleAspect.Rotate(SystemAPI.Time.DeltaTime);
            }*/
            
            // Third implementation option
            
            var rotatingCubeJob = new RotatingCollectibleJob { deltaTime = SystemAPI.Time.DeltaTime };
            
            state.Dependency = rotatingCubeJob.Schedule(state.Dependency);
        }
        
        [BurstCompile, WithAll(typeof(RotatingCollectible))]
        public partial struct RotatingCollectibleJob : IJobEntity
        {
            public float deltaTime;
            
            private void Execute(ref LocalTransform localTransform, in RotateSpeed rotateSpeed)
            {
                localTransform = localTransform.RotateY(rotateSpeed.value * deltaTime);
            }
        }
    }
}