using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace WPG.Turret.Gameplay
{
    public struct Cannon : IComponentData
    {
        public float MaxRotationSpeed;
        public float3 MouthOffset;
        public int CurrentAmmo;
        public float3 TargetPosition;
        public bool TargetSet;
        public bool TargetReached;
        public bool Active;
    }
    
    public readonly partial struct CannonAspect : IAspect
    {
        private readonly RefRW<Cannon> _cannon;
        public readonly TransformAspect Transform;

        public bool TargetSet => _cannon.ValueRO.TargetSet;

        public float3 TargetPosition => _cannon.ValueRO.TargetPosition;
        public float MaxRotationSpeed => _cannon.ValueRO.MaxRotationSpeed;
        public bool TargetReached => _cannon.ValueRO.TargetReached;
        public bool Active => _cannon.ValueRO.Active;

        public float3 MouthOffset => _cannon.ValueRO.MouthOffset;

        public int CurrentAmmo => _cannon.ValueRO.CurrentAmmo;

        public void UseAmmo()
        {
            _cannon.ValueRW.CurrentAmmo--;
        }

        public void SetTarget(float3 position)
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

        public void SetActive(bool value)
        {
            _cannon.ValueRW.Active = value;
        }
    }
}