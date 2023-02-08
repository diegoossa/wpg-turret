using Unity.Entities;
using UnityEngine;

namespace WPG.Turret.Gameplay
{
    public class BulletAuthoring : MonoBehaviour
    {
        public float Radius = 0.5f;
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, Radius);
        }
        
        private class BulletBaker : Baker<BulletAuthoring>
        {
            public override void Bake(BulletAuthoring authoring)
            {
                AddComponent(new Bullet
                {
                    Radius = authoring.Radius
                });
            }
        }
    }
}