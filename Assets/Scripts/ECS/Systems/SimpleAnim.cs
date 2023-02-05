using Unity.Burst;
using Unity.Entities;
using Unity.GPUAnimation;
using Unity.Mathematics;

namespace WPG.Turret.Gameplay
{
    public struct SimpleAnim : IComponentData
    {
        public int ClipIndex;
        public float Speed;
        public bool IsFirstFrame;
        public bool RandomizeStartTime;
        public float2 RandomizeMinMaxSpeed;
    }

    [BurstCompile]
    public partial struct SimpleAnimJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(ref SimpleAnim simple, ref GPUAnimationState animationState)
        {
            animationState.AnimationClipIndex = simple.ClipIndex;

            ref var clips = ref animationState.AnimationClipSet.Value.Clips;
            if ((uint)animationState.AnimationClipIndex < (uint)clips.Length)
            {
                if (!simple.IsFirstFrame)
                {
                    animationState.Time += DeltaTime * simple.Speed;
                }
                else
                {
                    var random = new Random();
                    random.InitState();
                    simple.Speed = random.NextFloat(simple.RandomizeMinMaxSpeed.x, simple.RandomizeMinMaxSpeed.y);
                    simple.IsFirstFrame = false;
                }
            }
        }
    }

    [BurstCompile]
    public partial struct SimpleAnimSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var job = new SimpleAnimJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}