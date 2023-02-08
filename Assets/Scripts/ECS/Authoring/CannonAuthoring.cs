using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace WPG.Turret.Gameplay
{
    public class CannonAuthoring : MonoBehaviour
    {
        public Transform MouthPosition;
        public float MaxRotationSpeed = 90f;
        public int StartingAmmo = 1000;
        
        private class CannonBaker : Baker<CannonAuthoring>
        {
            public override void Bake(CannonAuthoring authoring)
            {
                AddComponent(new Cannon
                {
                    // Rotation in Unity.Mathematics is in radians
                    MaxRotationSpeed = math.radians(authoring.MaxRotationSpeed),
                    MouthPosition = authoring.MouthPosition.position,
                    CurrentAmmo = authoring.StartingAmmo
                });
            }
        }
    }
}