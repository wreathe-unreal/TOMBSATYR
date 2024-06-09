using UnityEngine;
using UnityEngine.Rendering;

namespace PolyTerrains.Sources
{
    public static class MaterialSetup
    {
        private static readonly int TerrainWidthInvProperty = Shader.PropertyToID("_TerrainWidthInv");
        private static readonly int TerrainHeightInvProperty = Shader.PropertyToID("_TerrainHeightInv");
        private static readonly int EnableHeightBlend = Shader.PropertyToID("_EnableHeightBlend");
        private static readonly int HeightTransition = Shader.PropertyToID("_HeightTransition");

        private static int TxtCountPerPass => IsBuiltInHDRP() ? 8 : 4;

        public static int GetPassCount(TerrainData tData)
        {
            if (IsBuiltInHDRP())
                return 1;
            
            var passCount = tData.terrainLayers.Length / TxtCountPerPass;
            if (tData.terrainLayers.Length % TxtCountPerPass != 0) {
                passCount++;
            }

            return passCount;
        }
        
        private static bool IsBuiltInHDRP()
        {
#if USING_HDRP
            return true;
#else
            return false;
#endif
        }

        public static MaterialPropertyBlock SetupMaterial(int pass, Material material, Terrain terrain)
        {
            material.enableInstancing = false;

            var tData = terrain.terrainData;
            var prop = new MaterialPropertyBlock();
            terrain.GetSplatMaterialPropertyBlock(prop);

            var control = tData.GetAlphamapTexture(pass);
            prop.SetTexture($"_Control", control);
            prop.SetVector($"_Control_TexelSize", new Vector4(control.texelSize.x, control.texelSize.y, control.width, control.height));
            prop.SetFloat(TerrainWidthInvProperty, 1f / tData.size.x);
            prop.SetFloat(TerrainHeightInvProperty, 1f / tData.size.z);

            if (IsBuiltInHDRP()) {
                var control0 = tData.GetAlphamapTexture(0);
                prop.SetTexture($"_Control0", control0);
                prop.SetVector($"_Control0_TexelSize", new Vector4(control0.texelSize.x, control0.texelSize.y, control0.width, control0.height));
                var control1 = tData.GetAlphamapTexture(1);
                prop.SetTexture($"_Control1", control1);
                prop.SetVector($"_Control1_TexelSize", new Vector4(control1.texelSize.x, control1.texelSize.y, control1.width, control1.height));
                if (tData.terrainLayers.Length > 4) {
                    material.EnableKeyword("_TERRAIN_8_LAYERS");
                }
            }

            prop.SetFloat($"_NumLayersCount", tData.alphamapLayers);
            prop.SetTexture($"_TerrainHolesTexture", tData.holesTexture);

            if (tData.terrainLayers.Length <= 4 && terrain.materialTemplate.IsKeywordEnabled("_TERRAIN_BLEND_HEIGHT")) {
                material.EnableKeyword("_TERRAIN_BLEND_HEIGHT");
                prop.SetFloat(EnableHeightBlend, 1);
                prop.SetFloat(HeightTransition, terrain.materialTemplate.GetFloat(HeightTransition));
            } else {
                material.DisableKeyword("_TERRAIN_BLEND_HEIGHT");
                prop.SetFloat(EnableHeightBlend, 0);
            }

            var normalmap = false;
            var maskmap = false;
            var offset = pass * TxtCountPerPass;
            for (var i = 0; i + offset < tData.terrainLayers.Length && i < TxtCountPerPass; i++) {
                var terrainLayer = tData.terrainLayers[i + offset];
                if (terrainLayer == null || terrainLayer.diffuseTexture == null)
                    continue;

                if (terrainLayer.normalMapTexture)
                    normalmap = true;
                if (terrainLayer.maskMapTexture)
                    maskmap = true;

                prop.SetFloat($"_DiffuseHasAlpha{i}", 0);
                prop.SetFloat($"_NormalScale{i}", terrainLayer.normalScale);
                prop.SetFloat($"_LayerHasMask{i}", terrainLayer.maskMapTexture ? 1 : 0);
                prop.SetFloat($"_Metallic{i}", terrainLayer.metallic);
                prop.SetFloat($"_Smoothness{i}", terrainLayer.smoothness);
                prop.SetTexture($"_Splat{i}", terrainLayer.diffuseTexture);
                prop.SetVector($"_Splat{i}_ST", new Vector4( tData.size.x / terrainLayer.tileSize.x, tData.size.z / terrainLayer.tileSize.y, terrainLayer.tileOffset.x, terrainLayer.tileOffset.y));
                prop.SetVector($"_Splat{i}_TexelSize", new Vector4(terrainLayer.diffuseTexture.texelSize.x, terrainLayer.diffuseTexture.texelSize.y, terrainLayer.diffuseTexture.width, terrainLayer.diffuseTexture.height));
                if (terrainLayer.normalMapTexture)
                    prop.SetTexture($"_Normal{i}", terrainLayer.normalMapTexture);
                if (terrainLayer.maskMapTexture)
                    prop.SetTexture($"_Mask{i}", terrainLayer.maskMapTexture);
                prop.SetVector($"_MaskMapRemapOffset{i}", terrainLayer.maskMapRemapMin);
                prop.SetVector($"_MaskMapRemapScale{i}", terrainLayer.maskMapRemapMax);
                prop.SetVector($"_DiffuseRemapScale{i}", terrainLayer.diffuseRemapMax - terrainLayer.diffuseRemapMin);
            }

            if (normalmap) {
                material.EnableKeyword("_NORMALMAP");
            } else {
                material.DisableKeyword("_NORMALMAP");
            }

            if (maskmap) {
                material.EnableKeyword("_MASKMAP");
            } else {
                material.DisableKeyword("_MASKMAP");
            }

            return prop;
        }
    }
}