using System;
using UnityEngine;
using Unity.Entities;

[Serializable]
public struct RenderingData : ISharedComponentData
{
    public GameObject BakingPrefab;
    public Material Material;
    public LodData LodData;
}