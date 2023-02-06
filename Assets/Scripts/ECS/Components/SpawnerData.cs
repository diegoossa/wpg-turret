using Unity.Entities;
using Unity.Mathematics;

namespace WPG.Turret.Gameplay
{
    /// <summary>
    /// Data for the spawner
    /// </summary>
    public struct SpawnerData : IComponentData
    {
        public Entity Prefab;
        public float2 SpeedRange;
        public float2 CooldownRange;
        public float CurrentTimer;
    }
}