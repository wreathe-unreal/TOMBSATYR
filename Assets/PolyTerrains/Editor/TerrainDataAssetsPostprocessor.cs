using System.Collections;
using PolyTerrains.Sources;
using UnityEditor;
using UnityEngine;

namespace PolyTerrains.Editor
{
    internal class TerrainDataAssetsPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var terrains = Object.FindObjectsOfType<Terrain>();
            foreach (var terrain in terrains) {
                var assetPath = AssetDatabase.GetAssetPath(terrain.terrainData);
                if (((IList)importedAssets).Contains(assetPath)) {
                    var poly = terrain.GetComponent<PolyTerrain>();
                    if (poly) {
                        poly.RefreshMaterials();
                    }
                }
            }
        }
    }
}