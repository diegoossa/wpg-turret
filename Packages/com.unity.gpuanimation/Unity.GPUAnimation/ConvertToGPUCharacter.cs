using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Unity.GPUAnimation
{
	public static class CharacterUtility
	{
		public static BlobAssetReference<BakedAnimationClipSet> CreateClipSet(KeyframeTextureBaker.BakedData data)
		{
			using (var builder = new BlobBuilder(Allocator.Temp))
			{
				ref var root = ref builder.ConstructRoot<BakedAnimationClipSet>();
				var clips = builder.Allocate(ref root.Clips, data.Animations.Count);
				for (int i = 0; i != data.Animations.Count; i++)
					clips[i] = new BakedAnimationClip(data.AnimationTextures, data.Animations[i]);

				return builder.CreateBlobAssetReference<BakedAnimationClipSet>(Allocator.Persistent);
			}
		}

		public static void AddCharacterComponents(EntityManager manager, Entity entity, GameObject characterRig, AnimationClip[] clips, float framerate, LodData lod)
		{
			var renderer = characterRig.GetComponentInChildren<SkinnedMeshRenderer>();
			if (renderer == null)
			{
				return;
			}
			
			//@TODO: Perform validation that the shader supports GPU Skinning mode
			var bakedData = KeyframeTextureBaker.BakeClips(characterRig, clips, framerate, lod);

			var animState = default(GPUAnimationState);
			animState.AnimationClipSet = CreateClipSet(bakedData);
			manager.AddComponentData(entity, animState);
			manager.AddComponentData(entity, default(AnimationTextureCoordinate));

			var renderCharacter = new RenderCharacter
			{
				Material = renderer.sharedMaterial,
				AnimationTexture = bakedData.AnimationTextures,
				Mesh = bakedData.NewMesh,
				ReceiveShadows = renderer.receiveShadows,
				CastShadows = renderer.shadowCastingMode
				
			};
			manager.AddSharedComponentManaged(entity, renderCharacter);
		}
	}
   
}