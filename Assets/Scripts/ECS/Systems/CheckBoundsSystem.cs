using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace WPG.Turret.Gameplay
{
    [BurstCompile]
    public partial struct CheckBoundsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameBoard>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var gameBoard = GetSingleton<GameBoard>();

            foreach (var (transform, entity) in Query<TransformAspect>().WithEntityAccess().WithAll<MovementSpeed>())
            {
                if (!gameBoard.InsideBounds(transform.WorldPosition))
                {
                    commandBuffer.DestroyEntity(entity);
                }
            }
            commandBuffer.Playback(state.EntityManager);
        }
    }
}