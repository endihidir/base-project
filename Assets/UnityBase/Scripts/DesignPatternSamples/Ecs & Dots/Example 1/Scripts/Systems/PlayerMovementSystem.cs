using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace UnityBase.DesignPatterns.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct PlayerMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Movement>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var direction = new float3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            
            var playerMovementJob = new PlayerMovementJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                direction = direction
            };
            
            state.Dependency = playerMovementJob.Schedule(state.Dependency);
        }
        
        [BurstCompile, WithAll(typeof(Player)), WithDisabled(typeof(Stunned))]
        public partial struct PlayerMovementJob : IJobEntity
        {
            public float deltaTime;
            public float3 direction;
            
            private void Execute(ref LocalTransform localTransform, in Movement movement)
            {
                localTransform.Position += direction * movement.speed * deltaTime;
            }
        }
    }
}