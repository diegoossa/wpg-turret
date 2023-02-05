using System;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct RenderingData : ISharedComponentData, IEquatable<RenderingData>
{
    public GameObject BakingPrefab;
    public Material Material;
    public LodData LodData;

    public bool Equals(RenderingData other)
    {
        return Equals(BakingPrefab, other.BakingPrefab) && Equals(Material, other.Material) && LodData.Equals(other.LodData);
    }

    public override bool Equals(object obj)
    {
        return obj is RenderingData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BakingPrefab, Material, LodData);
    }
}