using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace WPG.Turret.Gameplay
{
    public struct Cannon : IComponentData
    {
        public float MaxRotationSpeed;
        public float3 MouthPosition;
        public int CurrentAmmo;
        public float3 TargetPosition;
        public bool TargetSet;
        public bool TargetReached;
    }
    
    public readonly partial struct CannonAspect : IAspect
    {
        private readonly RefRW<Cannon> _cannon;
        public readonly TransformAspect Transform;

        public bool TargetSet => _cannon.ValueRO.TargetSet;

        public float3 TargetPosition => _cannon.ValueRO.TargetPosition;
        public float MaxRotationSpeed => _cannon.ValueRO.MaxRotationSpeed;

        public bool TargetReached => _cannon.ValueRO.TargetReached;

        public void UseAmmo()
        {
            _cannon.ValueRW.CurrentAmmo--;
        }

        public void SetTarget(float3 position, Entity entity)
        {
            _cannon.ValueRW.TargetPosition = position;
            _cannon.ValueRW.TargetSet = true;
        }

        public void ReleaseTarget()
        {
            _cannon.ValueRW.TargetSet = false;
            _cannon.ValueRW.TargetReached = false;
        }

        public void ReachTarget()
        {
            _cannon.ValueRW.TargetReached = true;
        }
    }
}