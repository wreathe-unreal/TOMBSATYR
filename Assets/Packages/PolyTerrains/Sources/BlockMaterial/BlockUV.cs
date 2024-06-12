using System;
using Unity.Mathematics;

namespace PolyTerrains.Sources.BlockMaterial
{
    [Serializable]
    public struct BlockUV
    {
        public int TerrainLayerIndex;
        public float4 Top;
        public float4 Side;
        public float4 Bottom;

        public static BlockUV Default => new BlockUV
        {
            TerrainLayerIndex = 0,
            Top = new float4(0, 0, 1, 1),
            Side = new float4(0, 0, 1, 1),
            Bottom = new float4(0, 0, 1, 1),
        };
    }
}