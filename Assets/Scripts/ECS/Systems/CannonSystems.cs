using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace WPG.Turret.Gameplay
{
    public struct Targeted : IComponentData
    {
        public float Timer;
    }

    [BurstCompile]
    public partial struct SetCannonTargetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Cannon>();
            state.RequireForUpdate<Troll>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            // Created this system for one or multiple cannons
            foreach (var cannon in Query<CannonAspect>())
            {
                // If this cannon already has a target
                if (cannon.TargetSet || !cannon.Active)
                    continue;
                
                var minDistance = float.MaxValue;
                var target = float3.zero;
                var targetEntity = Entity.Null;

                foreach (var troll in Query<TrollAspect>()
                             .WithNone<Targeted>())
                {
                    var distance = math.distancesq(cannon.Transform.LocalPosition, troll.Position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        target = troll.Position;
                        targetEntity = troll.Entity;
                    }
                }

                if (targetEntity != Entity.Null)
                {
                    cannon.SetTarget(new float3(target.x, 0, target.z));
                    // Add targeted tag
                    commandBuffer.AddComponent(targetEntity, new Targeted {Timer = 2f});
                }
            }

            commandBuffer.Playback(state.EntityManager);
        }
    }

    [BurstCompile]
    [UpdateAfter(typeof(SetCannonTargetSystem))]
    public partial struct RotateCannonSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Cannon>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var cannon in Query<CannonAspect>())
            {
                if (cannon.TargetReached || !cannon.Active)
                    continue;
                
                var directionToTarget = math.normalize(cannon.TargetPosition - cannon.Transform.LocalPosition);
                directionToTarget.y = 0;
                var targetRotation = quaternion.LookRotationSafe(directionToTarget, math.up());
                cannon.Transform.LocalRotation = math.slerp(cannon.Transform.LocalRotation, targetRotation,
                    cannon.MaxRotationSpeed * SystemAPI.Time.DeltaTime);

                if (RotationEquals(cannon.Transform.LocalRotation, targetRotation))
                {
                    cannon.ReachTarget();
                }
            }
        }

        private bool RotationEquals(quaternion r1, quaternion r2)
        {
            var abs = math.abs(math.dot(r1, r2));
            return abs >= 0.9999f;
        }
    }

    [BurstCompile]
    [UpdateAfter(typeof(RotateCannonSystem))]
    public partial struct CannonBallSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Cannon>();
            state.RequireForUpdate<SpawnerData>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (cannon, spawner) in Query<CannonAspect, SpawnerAspect>())
            {
                if (!cannon.TargetReached || !cannon.TargetSet || !cannon.Active || cannon.CurrentAmmo <= 0)
                    continue;
                
                var cannonBall = commandBuffer.Instantiate(spawner.Prefab);
                var targetRotation = math.mul(cannon.Transform.LocalRotation, cannon.MouthOffset);
                var mouthPosition = cannon.Transform.LocalPosition + targetRotation;
                
                // Setup initial component values
                commandBuffer.SetComponent(cannonBall, new LocalTransform
                {
                    Position = mouthPosition,
                    Rotation = cannon.Transform.LocalRotation,
                    Scale = 1
                });

                commandBuffer.AddComponent(cannonBall, new MovementSpeed
                {
                    // Since the cannon ball velocity is not random we just use X
                    Value = spawner.SpeedRange.x
                });

                cannon.UseAmmo();
                cannon.ReleaseTarget();
            }

            commandBuffer.Playback(state.EntityManager);
        }
    }
    
    [BurstCompile]
    public partial struct RemoveTargetedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Targeted>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (targeted, entity) in Query<RefRW<Targeted>>().WithEntityAccess())
            {
                targeted.ValueRW.Timer -= SystemAPI.Time.DeltaTime;
                if (targeted.ValueRO.Timer <= 0)
                {
                    commandBuffer.RemoveComponent<Targeted>(entity);
                }
            }
                
            commandBuffer.Playback(state.EntityManager);
        }
    }
}