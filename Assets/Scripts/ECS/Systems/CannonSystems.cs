using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace WPG.Turret.Gameplay
{
    public struct TargetedTag : IComponentData
    {
    }

    [BurstCompile]
    public partial struct SetCannonTargetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Cannon>();
            state.RequireForUpdate<DamageableTag>();
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
                if(cannon.TargetSet)
                    continue;

                var minDistance = float.MaxValue;
                var target = float3.zero;
                var targetEntity = Entity.Null;

                foreach (var (damageableTransform, entity) in Query<TransformAspect>()
                             .WithAll<DamageableTag>()
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
            foreach (var (transform, cannon) in Query<TransformAspect, RefRO<Cannon>>())
            {
                float3 directionToTarget = math.normalize(cannon.ValueRO.TargetPosition - transform.LocalPosition);
                quaternion targetRotation = quaternion.LookRotation(directionToTarget, math.up());
                quaternion newRotation = math.slerp(transform.LocalRotation, targetRotation, cannon.ValueRO.MaxRotationSpeed * SystemAPI.Time.DeltaTime);
                transform.LocalRotation = newRotation;
            }
        }
    }
}