using Unity.Entities;
using Unity.Mathematics;

namespace WPG.Turret.Gameplay
{
    public struct MovementSpeed : IComponentData
    {
        public float Value;
    }
    
    public struct SpawnerData : IComponentData
    {
        public Entity Prefab;
        public float2 SpeedRange;
        public float2 TimeRange;
        public float CurrentTimer;
    }

    public struct GameBoard : IComponentData
    {
        public Bounds Bounds;
    }

    public struct Bounds
    {
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;

        public Bounds(float xMin, float xMax, float yMin, float yMax)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
        }
    }

    public struct Damageable : IComponentData
    {
    }

    public struct Cannon : IComponentData
    {
        
    }
}


