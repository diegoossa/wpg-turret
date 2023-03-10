using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace WPG.Turret.Gameplay
{
    /// <summary>
    /// Tag to differentiate Troll spawner
    /// </summary>
    public struct TrollSpawnerTag : IComponentData
    {
    }

    /// <summary>
    /// System to spawn trolls
    /// </summary>
    [BurstCompile]
    public partial struct TrollSpawnSystem : ISystem
    {
        private uint _seed;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameBoard>();
            state.RequireForUpdate<SpawnerData>();
            state.RequireForUpdate<TrollSpawnerTag>();

            _seed = (uint) Time.ElapsedTime;
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var random = new Random(++_seed * 100);
            var gameBoard = GetSingleton<GameBoard>();

            foreach (var spawner in Query<SpawnerAspect>().WithAll<TrollSpawnerTag>())
            {
                if (spawner.CurrentTimer > 0)
                {
                    spawner.DecreaseTimer(Time.DeltaTime);
                }
                else
                {
                    // Create troll instance
                    var instance = commandBuffer.Instantiate(spawner.Prefab);

                    // Calculate initial position and orientation
                    var position = new float3(random.NextFloat(gameBoard.Bounds.xMin, gameBoard.Bounds.xMax), 0,
                        random.NextFloat(gameBoard.Bounds.yMax, gameBoard.Bounds.yMax + gameBoard.SpawnerZone));
                    var target = new float3(random.NextFloat(gameBoard.Bounds.xMin, gameBoard.Bounds.xMax), 0,
                        gameBoard.Bounds.yMin);
                    var orientation = quaternion.LookRotation(target - position, math.up());

                    // Setup initial component values
                    commandBuffer.SetComponent(instance,
                        LocalTransform.FromPositionRotationScale(position, orientation, 1));

                    commandBuffer.AddComponent(instance,
                        new MovementSpeed
                        {
                            Value = random.NextFloat(spawner.SpeedRange.x, spawner.SpeedRange.y)
                        });

                    commandBuffer.AddComponent(instance, new Troll {Radius = 1f});

                    // Recalculate next timer
                    spawner.ResetTimer(random.NextFloat(
                        spawner.CooldownRange.x,
                        spawner.CooldownRange.y));
                }
            }

            commandBuffer.Playback(state.EntityManager);
        }
    }
}