using PolyTerrains.Sources;
using PolyTerrains.Sources.BlockMaterial;
using UnityEditor;
using UnityEngine;

namespace PolyTerrains.Editor
{
    [CustomEditor(typeof(PolyTerrain))]
    public class PolyTerrainEditor : UnityEditor.Editor
    {
        private PolyTerrain poly;

        public void OnEnable()
        {
            poly = (PolyTerrain)target;
        }

        private void ApplySettings()
        {
            poly.Refresh();
            SceneView.RepaintAll();
        }

        private void ApplyDraw()
        {
            poly.Update();
            SceneView.RepaintAll();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            poly.draw = EditorGUILayout.Toggle("Draw", poly.draw);
            if (EditorGUI.EndChangeCheck()) {
                ApplyDraw();
            }

            EditorGUI.BeginChangeCheck();
            poly.style = (PolyStyle)EditorGUILayout.EnumPopup("Style", poly.style);

            EditorGUILayout.Space();

            if (poly.materials == null || poly.materials.Length == 0)
                poly.materials = new Material[1];

            if (poly.style == PolyStyle.Blocky) {
                poly.blockUVsAsset = (BlockUVsAsset)EditorGUILayout.ObjectField("Block UVs Asset", poly.blockUVsAsset, typeof(BlockUVsAsset), false);
                poly.autoDisableTerrainCollider = EditorGUILayout.Toggle("Auto-Disable TerrainCollider", poly.autoDisableTerrainCollider);
            }

            GUI.enabled = false;
            poly.materials[0] = (Material)EditorGUILayout.ObjectField("Material", poly.materials[0], typeof(Material), false);
            GUI.enabled = true;
            EditorGUILayout.HelpBox(poly.style == PolyStyle.Blocky ? "The material is defined by the Block Uvs Asset." : "The material is directly taken from the terrain.", MessageType.Info);

            EditorGUILayout.Space();

            poly.enableOcclusionCulling = EditorGUILayout.Toggle("Enable Occlusion Culling", poly.enableOcclusionCulling);
            poly.layer = EditorGUILayout.LayerField("Layer", poly.layer);
            poly.ChunkSize = EditorGUILayout.IntPopup("Chunk size", poly.ChunkSize, new[] { "16", "32", "64" },
                new[] { 17, 33, 65 });

            if (EditorGUI.EndChangeCheck()) {
                ApplySettings();
            }

            if (GUILayout.Button("Refresh")) {
                ApplySettings();
            }
        }
    }
}