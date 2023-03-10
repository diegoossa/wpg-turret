using Unity.Entities;
using Unity.Mathematics;

namespace WPG.Turret.Gameplay
{
    public struct GameBoard : IComponentData
    {
        public Bounds Bounds;
        // Added a depth for the spawner box, so it is possible to spawn more trolls
        public float SpawnerZone;
        // 
        private const float SafeMargin = 1f;
        
        public bool InsideBounds(float3 position)
        {
            return position.x > Bounds.xMin - SafeMargin && position.x < Bounds.xMax + SafeMargin &&
                       position.z > Bounds.yMin - SafeMargin && position.z < Bounds.yMax + SpawnerZone + SafeMargin;
        }
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
}