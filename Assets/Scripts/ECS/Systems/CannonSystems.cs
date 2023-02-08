using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace WPG.Turret.Gameplay
{
    public struct TargetedTag : IComponentData { }

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
                if (cannon.TargetSet)
                    continue;

                var minDistance = float.MaxValue;
                var target = float3.zero;
                var targetEntity = Entity.Null;

                foreach (var (damageableTransform, entity) in Query<TransformAspect>()
                             .WithAll<Troll>()
                             .WithNone<TargetedTag>()
                             .WithEntityAccess())
                {
                    var distance = math.distancesq(cannon.Transform.WorldPosition, damageableTransform.WorldPosition);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        target = damageableTransform.WorldPosition;
                        targetEntity = entity;
                    }
                }

                if (targetEntity != Entity.Null)
                {
                    cannon.SetTarget(target, targetEntity);
                    // Add targeted tag
                    commandBuffer.AddComponent<TargetedTag>(targetEntity);
                }
            }

            commandBuffer.Playback(state.EntityManager);
        }
    }

    [BurstCompile]
    public partial struct RotateCannonSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Cannon>();
            state.RequireForUpdate<TargetedTag>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var cannon in Query<CannonAspect>())
            {
                if (!cannon.TargetSet || cannon.TargetReached)
                    continue;

                var directionToTarget = math.normalize(cannon.TargetPosition - cannon.Transform.LocalPosition);
                var targetRotation = quaternion.LookRotationSafe(directionToTarget, math.up());
                cannon.Transform.LocalRotation = math.slerp(cannon.Transform.LocalRotation, targetRotation,
                    cannon.MaxRotationSpeed * Time.DeltaTime);

                if (RotationEquals(cannon.Transform.LocalRotation, targetRotation))
                {
                    cannon.ReachTarget();
                }
            }
        }

        private bool RotationEquals(quaternion r1, quaternion r2)
        {
            var abs = math.abs(math.dot(r1, r2));
            return abs >= 0.999f;
        }
    }

    [BurstCompile]
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
                if (!cannon.TargetReached || !cannon.TargetSet) 
                    continue;
                
                var cannonBall = commandBuffer.Instantiate(spawner.Prefab);

                // Setup initial component values
                commandBuffer.SetComponent(cannonBall, new LocalTransform
                {
                    Position = cannon.Transform.LocalPosition,
                    Rotation = cannon.Transform.LocalRotation,
                    Scale = 1
                });

                commandBuffer.AddComponent(cannonBall, new MovementSpeed
                {
                    // Since the cannon ball velocity is not random we just use X
                    Value = spawner.SpeedRange.x
                });
                
                cannon.ReleaseTarget();
            }
            
            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }
    }
}