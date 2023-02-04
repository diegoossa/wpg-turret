using Unity.Entities;
using Unity.Entities.UI;
using Unity.Mathematics;
using UnityEngine;

namespace WPG.Turret.Gameplay
{
    public class SpawnerAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        [MinMax(0f, 5f)]
        public float2 SpeedRange;
        
        private class SpawnerBaker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                
            }
        }
    }
}