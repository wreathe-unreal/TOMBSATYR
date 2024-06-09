using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace PolyTerrains.Sources.BlockMaterial
{
    [AddComponentMenu("PolyTerrains/Block Material Maker")]
    public class BlockMaterialMaker : MonoBehaviour
    {
        [SerializeField] public int selectedTerrainLayerIndex;
        [SerializeField] private List<Block> blocks = new List<Block>();

        public List<Block> Blocks => blocks;

        public Texture2D GenerateAtlas(out List<BlockUV> blockUVs)
        {
            var atlas = new Texture2D(8192, 8192);
            var textures = new List<Texture2D>();
            foreach (var block in blocks) {
                textures.Add(block.top);
                textures.Add(block.side);
                textures.Add(block.bottom);
            }

            var rects = atlas.PackTextures(textures.ToArray(), 0, atlas.width);
            atlas.Apply();

            blockUVs = new List<BlockUV>();
            for (var i = 0; i < blocks.Count; i++) {
                var block = blocks[i];
                if (!block) {
                    blockUVs.Add(new BlockUV
                    {
                        TerrainLayerIndex = i,
                        Top = new float4(0, 0, 1, 1),
                        Side = new float4(0, 0, 1, 1),
                        Bottom = new float4(0, 0, 1, 1)
                    });
                    continue;
                }

                var rT = rects[3 * i + 0];
                var rS = rects[3 * i + 1];
                var rB = rects[3 * i + 2];
                blockUVs.Add(new BlockUV
                {
                    TerrainLayerIndex = i,
                    Top = new float4(rT.min.x, rT.min.y, rT.max.x, rT.max.y),
                    Side = new float4(rS.min.x, rS.min.y, rS.max.x, rS.max.y),
                    Bottom = new float4(rB.min.x, rB.min.y, rB.max.x, rB.max.y)
                });
            }

            return atlas;
        }
    }
}