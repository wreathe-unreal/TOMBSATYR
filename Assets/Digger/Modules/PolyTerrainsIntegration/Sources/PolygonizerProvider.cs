using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.Polygonizers;
using PolyTerrains.Sources;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Digger.Modules.Core.Editor;
using UnityEditor;
#endif

namespace Digger.Modules.PolyTerrainsIntegration.Sources
{
    [AddComponentMenu("PolyTerrains/Digger Polygonizer Provider")]
    public class PolygonizerProvider : APolygonizerProvider
    {
#if UNITY_EDITOR
        private void Reset()
        {
            var diggerMaster = GetComponent<DiggerMaster>();
            if (diggerMaster && !diggerMaster.AutoVoxelHeight && EditorUtility.DisplayDialog(
                    "Change voxel height & clear everything",
                    "In order to make Digger work properly with PolyTerrains, 'Auto Voxel Height' must be enabled.\n\n" +
                    "Do you want to enable 'Auto Voxel Height' now?\n\n" +
                    "THIS WILL CLEAR ALL MODIFICATIONS MADE WITH DIGGER.\n" +
                    "This operation CANNOT BE UNDONE.\n\n" +
                    "Are you sure you want to proceed?", "Yes", "Cancel")) {
                diggerMaster.AutoVoxelHeight = true;
                ClearAllDigger();
            }
        }

        private static void ClearAllDigger()
        {
            var diggers = FindObjectsOfType<DiggerSystem>();

            try {
                AssetDatabase.StartAssetEditing();
                foreach (var digger in diggers) {
                    digger.Clear();
                }
            } finally {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();

            try {
                AssetDatabase.StartAssetEditing();
                foreach (var digger in diggers) {
                    DiggerSystemEditor.Init(digger, true);
                    Undo.ClearUndo(digger);
                }
            } finally {
                AssetDatabase.StopAssetEditing();
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
#endif

        public override IPolygonizer NewPolygonizer(DiggerSystem digger)
        {
            var poly = FindObjectOfType<PolyTerrain>();
            if (!poly || !poly.draw)
                return new MarchingCubesPolygonizer(); // fallback to Marching Cubes

            switch (poly.style) {
                case PolyStyle.LowPoly:
                    return new MarchingCubesPolygonizer(true);
                case PolyStyle.Blocky:
                    return new BlockyPolygonizer(poly.blockUVsAsset);
                default:
                    return new MarchingCubesPolygonizer(); // fallback to Marching Cubes
            }
        }

        public override Material[] GetMaterials()
        {
            var poly = FindObjectOfType<PolyTerrain>();
            if (!poly || !poly.draw)
                return null;

            switch (poly.style) {
                case PolyStyle.LowPoly:
                    return null;
                case PolyStyle.Blocky:
                    return poly.materials;
                default:
                    return null;
            }
        }
    }
}