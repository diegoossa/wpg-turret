using Unity.Entities;
using UnityEngine;

namespace WPG.Turret.Gameplay
{
    public class TrollSpawnerAuthoring : MonoBehaviour
    {
        private class TrollSpawnerBaker : Baker<TrollSpawnerAuthoring>
        {
            public override void Bake(TrollSpawnerAuthoring authoring)
            {
                AddComponent<TrollSpawnerTag>();
            }
        }
    }
}