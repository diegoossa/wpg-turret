using Unity.Entities;
using Unity.GPUAnimation;
using Unity.Mathematics;
using UnityEngine;

namespace WPG.Turret.Gameplay
{
    /// <summary>
    /// Creates Unit entity and apply gpu animation baking  
    /// </summary>
    public class UnitAuthoring : MonoBehaviour
    {
        // GPU animation parameters 
        public AnimationClip[] Clips;
        public float AnimationFrameRate = 60.0f;
        public bool RandomizeStartTime;
        public int ClipIndex;
        public float2 RandomizeMinMaxSpeed = new(1f, 1f);
        
        // GPU animation mesh data
        public RenderingData RenderingInfo;
    }
    
    [TemporaryBakingType]
    public struct UnitBakingData : IComponentData
    {
        public UnityObjectRef<UnitAuthoring> Authoring;
    }
    
    public class UnitBaker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            AddComponent(new UnitBakingData()
            {
                Authoring = authoring
            });
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    internal partial class UnitBakingSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithEntityQueryOptions(EntityQueryOptions.IncludePrefab)
                .ForEach((Entity entity, in UnitBakingData unitBakingData) =>
            {
                var authoring = unitBakingData.Authoring.Value;
                EntityManager.AddComponent<GPUAnimationState>(entity);
                EntityManager.AddComponent<AnimationTextureCoordinate>(entity);

                // Use GPU animation utils to add the corresponding components to the BakingPrefab entity
                CharacterUtility.AddCharacterComponents(EntityManager, entity, authoring.RenderingInfo.BakingPrefab, authoring.Clips, authoring.AnimationFrameRate, authoring.RenderingInfo.LodData);

                EntityManager.AddComponentData(entity, new SimpleAnim
                {
                    RandomizeStartTime = authoring.RandomizeStartTime,
                    ClipIndex = authoring.ClipIndex,
                    Speed = 1.0F,
                    IsFirstFrame = true,
                    RandomizeMinMaxSpeed = authoring.RandomizeMinMaxSpeed
                });
            }).WithStructuralChanges().Run();
        }
    }
}