using Unity.Entities;
using Unity.Mathematics;

namespace WPG.Turret.Gameplay
{
    public struct SpawnerData : IComponentData
    {
        public Entity Prefab;
        public float2 SpeedRange;
        public float2 TimeRange;
        public float CurrentTimer;
    }
}