using Unity.Entities;

namespace WPG.Turret.Gameplay
{
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
}