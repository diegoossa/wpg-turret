using Unity.Entities;
using UnityEngine;

namespace WPG.Turret.Gameplay
{
    public class TrollVisualAuthoring : MonoBehaviour
    {
        private class TrollVisualBaker : Baker<TrollVisualAuthoring>
        {
            public override void Bake(TrollVisualAuthoring authoring)
            {
                AddComponent<TrollVisual>();
            }
        }
    }
}