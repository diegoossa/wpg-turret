using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Unity.GPUAnimation
{
	public struct GPUAnimationState : IComponentData
	{
		public float Time;
		public int   AnimationClipIndex;
		public int NextAnimationClipIndex;
		public BlobAssetReference<BakedAnimationClipSet> AnimationClipSet;
	}
	
	public struct AnimationTextureCoordinate : IComponentData
	{
		public float3 Coordinate;
	}
	
	
	public struct BakedAnimationClipSet
	{
		public BlobArray<BakedAnimationClip> Clips;
	}

	public struct BakedAnimationClip
	{
		internal float TextureOffset;
		internal float TextureRange;
		internal float OnePixelOffset;
		internal float TextureWidth;
		internal float OneOverTextureWidth;
		internal float OneOverPixelOffset;

		public float AnimationLength;
		public bool  Looping;

		public BakedAnimationClip(AnimationTextures animTextures, KeyframeTextureBaker.AnimationClipData clipData)
		{
			float onePixel = 1f / animTextures.Animation0.width;
			float start = (float)clipData.PixelStart / animTextures.Animation0.width;
			float end = (float)clipData.PixelEnd / animTextures.Animation0.width;

			TextureOffset = start;
			TextureRange = end - start;
			OnePixelOffset = onePixel;
			TextureWidth = animTextures.Animation0.width;
			OneOverTextureWidth = 1.0F / TextureWidth;
			OneOverPixelOffset = 1.0F / OnePixelOffset;
			
			AnimationLength = clipData.Clip.length;
			Looping = clipData.Clip.wrapMode == WrapMode.Loop;
		}
		
		public float3 ComputeCoordinate(float normalizedTime)
		{
			float texturePosition = normalizedTime * TextureRange + TextureOffset;
			float lowerPixelFloor = math.floor(texturePosition * TextureWidth);

			float lowerPixelCenter = lowerPixelFloor * OneOverTextureWidth;
			float upperPixelCenter = lowerPixelCenter + OnePixelOffset;
			float lerpFactor = (texturePosition - lowerPixelCenter) * OneOverPixelOffset;

			return  new float3(lowerPixelCenter, upperPixelCenter, lerpFactor);
		}
		
		public float ComputeNormalizedTime(float time)
		{
			if (Looping)
				return Mathf.Repeat(time, AnimationLength) / AnimationLength;
			else
				return math.saturate(time / AnimationLength);
		}

	}

	[System.Serializable]
	struct RenderCharacter : ISharedComponentData, IEquatable<RenderCharacter>
	{
		//@TODO: Would be nice if we had BlobAssetReference in shared component data support (Serialize not supported...) 
		public Material                                  Material;
		public AnimationTextures                         AnimationTexture;
		public Mesh                                      Mesh;
		public bool                                      ReceiveShadows;
		public ShadowCastingMode                         CastShadows;
		
		public bool Equals(RenderCharacter other)
		{
			return Material == other.Material && AnimationTexture.Equals(other.AnimationTexture) && Mesh == other.Mesh && ReceiveShadows == other.ReceiveShadows && CastShadows == other.CastShadows;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (ReferenceEquals(Material, null) ? 0 : Material.GetHashCode());
				hashCode = (hashCode * 397) ^ AnimationTexture.GetHashCode();
				hashCode = (hashCode * 397) ^ (ReferenceEquals(Mesh, null) ? 0 : Mesh.GetHashCode());
				return hashCode;
			}
		}
	}

	unsafe public static class NativeExtensionTemp
	{
        public static NativeArray<U> Reinterpret_Temp<T, U>(this NativeArray<T> array) where U : struct where T : struct
        {
            var tSize = UnsafeUtility.SizeOf<T>();
            var uSize = UnsafeUtility.SizeOf<U>();

             var byteLen = ((long) array.Length) * tSize;
            var uLen = byteLen / uSize;

 #if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (uLen * uSize != byteLen)
            {
                throw new InvalidOperationException($"Types {typeof(T)} (array length {array.Length}) and {typeof(U)} cannot be aliased due to size constraints. The size of the types and lengths involved must line up.");
            }

 #endif
            var ptr = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array);
            var result = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<U>(ptr, (int) uLen, Allocator.Invalid);

 #if ENABLE_UNITY_COLLECTIONS_CHECKS
            var handle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(array);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref result, handle);
#endif

             return result;
        }

	}
	
	[BurstCompile]
	struct CalculateTextureCoordJob : IJobChunk
	{
		public ComponentTypeHandle<GPUAnimationState> gpuAnimationStateType;
		public ComponentTypeHandle<AnimationTextureCoordinate> animationTextureCoordType;

		public void Execute(in ArchetypeChunk chunk, int chunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			var animStates = chunk.GetNativeArray(gpuAnimationStateType);
			var textureCoordinates = chunk.GetNativeArray(animationTextureCoordType);

			for (var i = 0; i < chunk.Count; i++)
			{
				var textureCoordinate = textureCoordinates[i];
				var animstate = animStates[i];
				if (animstate.AnimationClipIndex != animstate.NextAnimationClipIndex)
				{
					animstate.AnimationClipIndex = animstate.NextAnimationClipIndex;
						
				}
					
				ref var clips = ref animstate.AnimationClipSet.Value.Clips;
				if ((uint) animstate.AnimationClipIndex < (uint) clips.Length)
				{
					var normalizedTime = clips[animstate.AnimationClipIndex].ComputeNormalizedTime(animstate.Time);
					textureCoordinate.Coordinate = clips[animstate.AnimationClipIndex].ComputeCoordinate(normalizedTime);
				}
				else
				{
					// How to warn???
				}

				textureCoordinates[i] = textureCoordinate;
				animStates[i] = animstate;
			}
		}
	}
	
	[BurstCompile]
	public partial struct CalculateTextureCoordinateSystem : ISystem
	{
		private EntityQuery _query;
		private ComponentTypeHandle<AnimationTextureCoordinate> _animationTextureCoordinateHandle;
		private ComponentTypeHandle<GPUAnimationState> _gpuAnimationStateHandle;
		public void OnCreate(ref SystemState state)
		{
			_query = state.GetEntityQuery(
				ComponentType.ReadOnly<GPUAnimationState>(), 
				ComponentType.ReadOnly<AnimationTextureCoordinate>());
			_animationTextureCoordinateHandle = state.GetComponentTypeHandle<AnimationTextureCoordinate>();
			_gpuAnimationStateHandle = state.GetComponentTypeHandle<GPUAnimationState>();
		}

		public void OnDestroy(ref SystemState state)
		{

		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			_animationTextureCoordinateHandle.Update(ref state);
			_gpuAnimationStateHandle.Update(ref state);
			var calculateTextureCoordJob = new CalculateTextureCoordJob
			{
				animationTextureCoordType = _animationTextureCoordinateHandle,
				gpuAnimationStateType = _gpuAnimationStateHandle
			};
			state.Dependency = calculateTextureCoordJob.ScheduleParallel(_query, state.Dependency);
		}
	}
	
	[UpdateAfter(typeof(CalculateTextureCoordinateSystem))]
	public partial class GpuCharacterRenderSystem : SystemBase
    {
	    private List<RenderCharacter> _Characters = new List<RenderCharacter>();
	    private Dictionary<RenderCharacter, InstancedSkinningDrawer> _Drawers = new Dictionary<RenderCharacter, InstancedSkinningDrawer>();

	    private EntityQuery m_Characters;


	    protected override void OnUpdate()
        {
	        _Characters.Clear();
	        EntityManager.GetAllUniqueSharedComponentsManaged(_Characters);

	        foreach (var character in _Characters)
	        {
		        if (character.Material == null || character.Mesh == null)
			        continue;
		        
		        //@TODO: Currently we never cleanup the _Drawers cache when the last entity with that renderer disappears.
		        InstancedSkinningDrawer drawer;
		        if (!_Drawers.TryGetValue(character, out drawer))
		        {
			        drawer = new InstancedSkinningDrawer(character.Material, character.Mesh, character.AnimationTexture);
			        _Drawers.Add(character, drawer);
		        }
		        
				m_Characters.SetSharedComponentFilterManaged(character);

				Profiler.BeginSample("ExtractState");
				JobHandle jobA, jobB;
		        var coords = m_Characters.ToComponentDataListAsync<AnimationTextureCoordinate>(World.UpdateAllocator.ToAllocator, out jobA);
		        m_Characters.AddDependency(jobA);
		        
		        var localToWorld = m_Characters.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob, out jobB);
		        m_Characters.AddDependency(jobB);
		        
		        JobHandle.CompleteAll(ref jobA, ref jobB);
		        Profiler.EndSample();
		        
		        drawer.Draw(coords.ToArray(Allocator.TempJob).Reinterpret_Temp<AnimationTextureCoordinate, float3>(), localToWorld.ToArray(Allocator.TempJob).Reinterpret_Temp<LocalToWorld, float4x4>(), character.CastShadows, character.ReceiveShadows);
		        coords.Dispose();
		        localToWorld.Dispose();
	        }
	        
        }

        protected override void OnCreate()
        {
	        m_Characters = GetEntityQuery(ComponentType.ReadOnly<RenderCharacter>(), ComponentType.ReadOnly<GPUAnimationState>(), ComponentType.ReadOnly<LocalToWorld>(), ComponentType.ReadOnly<AnimationTextureCoordinate>());
        }

        protected override void OnDestroy()
        {
	        foreach(var drawer in _Drawers.Values)
		        drawer.Dispose();
	        _Drawers = null;
        }
    }
}