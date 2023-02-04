using Unity.Entities;
using Unity.Entities.UI;
using Unity.Mathematics;
using UnityEngine;

namespace WPG.Turret.Gameplay
{
    public class SpawnerAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        [MinMax(0f, 5f)] public float2 SpeedRange;
        [MinMax(1f, 10f)] public float2 SpawnTime;

        private class SpawnerBaker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                AddComponent(new SpawnerData
                {
                    Prefab = GetEntity(authoring.Prefab),
                    SpeedRange = authoring.SpeedRange,
                    TimeRange = authoring.SpawnTime
                });
            }
        }
    }
}