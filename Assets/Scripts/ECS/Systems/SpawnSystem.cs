using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace WPG.Turret.Gameplay
{
    public partial struct SpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameBoard>();
            state.RequireForUpdate<SpawnerData>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var random = new Random((uint)Time.ElapsedTime + 1);
            var gameBoard = GetSingleton<GameBoard>();

            foreach (var spawnerData in Query<RefRW<SpawnerData>>())
            {
                if (spawnerData.ValueRO.CurrentTimer > 0)
                {
                    spawnerData.ValueRW.CurrentTimer -= Time.DeltaTime;
                }
                else
                {
                    var instance = commandBuffer.Instantiate(spawnerData.ValueRO.Prefab);
                    var position = new float3(random.NextFloat(gameBoard.Bounds.xMin, gameBoard.Bounds.xMax), 0,
                        gameBoard.Bounds.yMax);
                    commandBuffer.SetComponent(instance, new LocalTransform
                    {
                        Position = position,
                        Rotation = quaternion.identity,
                        Scale = 1
                    });

                    // commandBuffer.AddComponent(instance,
                    //     new MovementSpeed
                    //     {
                    //         Value = random.NextFloat(spawnerData.ValueRO.SpeedRange.x, spawnerData.ValueRO.SpeedRange.y)
                    //     });

                    spawnerData.ValueRW.CurrentTimer = random.NextFloat(
                        spawnerData.ValueRO.TimeRange.x,
                        spawnerData.ValueRO.TimeRange.y);

                    state.Enabled = false;
                }
            }

            commandBuffer.Playback(state.EntityManager);
        }
    }
}