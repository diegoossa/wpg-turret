using Unity.Entities;
using Unity.Mathematics;

namespace WPG.Turret.Gameplay
{
    public struct Cannon : IComponentData
    {
        public float MaxRotationSpeed;
        public float3 MouthPosition;
        public int CurrentAmmo;
        public float3 CurrentTarget;
    }
}