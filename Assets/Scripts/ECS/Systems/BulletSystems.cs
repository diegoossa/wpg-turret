using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Entities.SystemAPI;

namespace WPG.Turret.Gameplay
{
    [BurstCompile]
    public partial struct CheckCannonBallCollisionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }
    
        public void OnDestroy(ref SystemState state)
        {
        }
    
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
    
            foreach (var troll in Query<TrollAspect>())
            {
                foreach (var bullet in Query<BulletAspect>())
                {
                    var distance = math.length(bullet.Position - troll.Position);
                    var combinedRadius = bullet.Radius + troll.Radius;
                    
                    if (distance <= combinedRadius)
                    {
                        commandBuffer.DestroyEntity(troll.Entity);
                        commandBuffer.DestroyEntity(bullet.Entity);
                    }
                }
            }
            
            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }
    }
}