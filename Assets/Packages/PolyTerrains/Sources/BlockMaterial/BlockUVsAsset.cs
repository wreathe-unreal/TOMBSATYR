using System;
using UnityEngine;

namespace PolyTerrains.Sources.BlockMaterial
{
    [Serializable]
    public class BlockUVsAsset : ScriptableObject
    {
        [SerializeField] public BlockUV[] blockUVs;
        [SerializeField] public Texture2D atlas;
        [SerializeField] public Material material;
    }
}