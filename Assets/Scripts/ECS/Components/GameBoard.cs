using Unity.Entities;
using Unity.Mathematics;

namespace WPG.Turret.Gameplay
{
    public struct GameBoard : IComponentData
    {
        public Bounds Bounds;
        // Added a depth for the spawner box, so it is possible to spawn more trolls
        public float SpawnerZone;
        public bool InsideBounds(float3 position)
        {
            return position.x > Bounds.xMin && position.x < Bounds.xMax &&
                   position.z > Bounds.yMin && position.z < Bounds.yMax;
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