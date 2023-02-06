using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace WPG.Turret.Gameplay
{
    public struct Cannon : IComponentData
    {
        public float MaxRotationSpeed;
        public float3 MouthPosition;
        public int CurrentAmmo;
        public float3 TargetPosition;
        public Entity TargetEntity;
        public bool TargetSet;
    }
    
    public readonly partial struct CannonAspect : IAspect
    {
        private readonly RefRW<Cannon> _cannon;
        public readonly TransformAspect Transform;

        public bool TargetSet => _cannon.ValueRO.TargetSet;

        public void UseAmmo()
        {
            _cannon.ValueRW.CurrentAmmo--;
        }

        public void SetTarget(float3 position, Entity entity)
        {
            _cannon.ValueRW.TargetPosition = position;
            _cannon.ValueRW.TargetEntity = entity;
            _cannon.ValueRW.TargetSet = true;
        }

        public void ReleaseTarget()
        {
            _cannon.ValueRW.TargetSet = false;
        }
    }
}