using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace WPG.Turret.Gameplay
{
    public class GameBoardAuthoring : MonoBehaviour
    {
        public Transform BottomLeft;
        public Transform TopRight;
        public float SpawnerZone = 5f;
        
        private class GameBoardBaker : Baker<GameBoardAuthoring>
        {
            public override void Bake(GameBoardAuthoring authoring)
            {
                var bottomLeftPosition = authoring.BottomLeft.position;
                var topRightPosition = authoring.TopRight.position;
                AddComponent(new GameBoard
                {
                    Bounds = new Bounds(
                        bottomLeftPosition.x,
                        topRightPosition.x,
                        bottomLeftPosition.z,
                        topRightPosition.z),
                    SpawnerZone = authoring.SpawnerZone
                });
            }
        }
    }
}