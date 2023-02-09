using Unity.Burst;
using Unity.Entities;

namespace WPG.Turret.Gameplay
{
    [BurstCompile]
    public partial struct CheckTrollPositionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Troll>();
            state.RequireForUpdate<Cannon>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            // In case there are multiple cannons
            foreach (var cannon in SystemAPI.Query<CannonAspect>())
            {
                foreach (var troll in SystemAPI.Query<TrollAspect>())
                {
                    if (troll.Position.z <= cannon.Transform.LocalPosition.z)
                    {
                        cannon.SetActive(false);
                        HUDController.Instance.GameOver();
                    }
                }
            }
        }
    }
}