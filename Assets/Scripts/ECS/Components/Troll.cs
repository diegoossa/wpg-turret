using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace WPG.Turret.Gameplay
{
    public struct Troll : IComponentData
    {
        public float Radius;
    }

    public readonly partial struct TrollAspect : IAspect
    {
        public readonly Entity Entity;
        private readonly RefRO<Troll> _troll;
        public readonly TransformAspect Transform;
        public float Radius => _troll.ValueRO.Radius;
        public float3 Position => Transform.LocalPosition;
    }
}