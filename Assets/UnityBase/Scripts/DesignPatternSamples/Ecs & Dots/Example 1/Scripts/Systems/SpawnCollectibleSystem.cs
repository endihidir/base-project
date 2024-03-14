using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityBase.DesignPatterns.ECS
{
    public partial class SpawnCollectibleSystem : SystemBase
    {
        private bool _isSpawned, _isPlayerActivated;
        
        protected override void OnCreate()
        {
            //RequireForUpdate<CollectibleSpawn>();
        }

        protected override void OnUpdate()
        {
            /*try
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    _isPlayerActivated = !_isPlayerActivated;
                    var playerEntity = SystemAPI.GetSingletonEntity<Player>();
                    EntityManager.SetComponentEnabled<Stunned>(playerEntity, _isPlayerActivated);
                }
                
                if(_isSpawned) return;

                _isSpawned = true;

                var spawnCollectible = SystemAPI.GetSingleton<CollectibleSpawn>();

                var entityCommandBuffer = new EntityCommandBuffer(WorldUpdateAllocator);
            
                for (int i = 0; i < spawnCollectible.amountToSpawn; i++)
                {
                    // Use this
                    /*var spawnedEntity = EntityManager.Instantiate(spawnCollectible.cubePrefabEntity);
                    
                    EntityManager.SetComponentData(spawnedEntity, new LocalTransform
                    {
                        Position = new float3(Random.Range(-5f, 5f), 0.6f, Random.Range(-5f, 5f)),
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });#1#
                
                
                    // Or use this
                
                    var spawnedEntity = entityCommandBuffer.Instantiate(spawnCollectible.cubePrefabEntity);
                
                    entityCommandBuffer.SetComponent(spawnedEntity, new LocalTransform
                    {
                        Position = new float3(Random.Range(-5f, 5f), 0.6f, Random.Range(-5f, 5f)),
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });
                }
            
                entityCommandBuffer.Playback(EntityManager);
            
                // You dont need below statement because this is SystemBase and Allocator.Temp combination.
            
                //entityCommandBuffer.Dispose();
            }
            catch (InvalidOperationException e)
            {
                Debug.Log(e);
                Enabled = false;
            }*/
        }
    }
}