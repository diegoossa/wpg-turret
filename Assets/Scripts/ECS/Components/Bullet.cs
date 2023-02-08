using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace WPG.Turret.Gameplay
{
    public struct Bullet : IComponentData
    {
        public float Radius;
    }
    
    public readonly partial struct BulletAspect : IAspect
    {
        public readonly Entity Entity;
        private readonly RefRO<Bullet> _bullet;
        public readonly TransformAspect Transform;

        public float Radius => _bullet.ValueRO.Radius;
        public float3 Position => Transform.LocalPosition;
    }
}