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

    public readonly partial struct SpawnerAspect : IAspect
    {
        private readonly RefRW<SpawnerData> _spawner;

        public Entity Prefab => _spawner.ValueRO.Prefab;
        public float2 SpeedRange => _spawner.ValueRO.SpeedRange;
        public float2 CooldownRange => _spawner.ValueRO.CooldownRange;
        public float CurrentTimer => _spawner.ValueRO.CurrentTimer;

        public void DecreaseTimer(float value)
        {
            _spawner.ValueRW.CurrentTimer -= value;
        }

        public void ResetTimer(float value)
        {
            _spawner.ValueRW.CurrentTimer = value;
        }
    }
}