using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;

namespace WPG.Turret.Gameplay
{
    /// <summary>
    /// System to move forward all the entities that has MovementSpeed
    /// </summary>
    [BurstCompile]
    public partial struct MoveForwardSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MovementSpeed>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (transform, movementSpeed) in Query<TransformAspect, RefRO<MovementSpeed>>())
            {
                transform.TranslateWorld(transform.LocalTransform.Forward() * movementSpeed.ValueRO.Value *
                                         SystemAPI.Time.DeltaTime);
            }
        }
    }
}