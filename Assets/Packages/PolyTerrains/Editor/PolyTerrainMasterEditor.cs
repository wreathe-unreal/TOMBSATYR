using PolyTerrains.Sources;
using PolyTerrains.Sources.BlockMaterial;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PolyTerrains.Editor
{
    [CustomEditor(typeof(PolyTerrainMaster))]
    public class PolyTerrainMasterEditor : UnityEditor.Editor
    {
        private PolyTerrainMaster master;

        public void OnEnable()
        {
            master = (PolyTerrainMaster)target;
        }

        [MenuItem("Tools/PolyTerrain/Setup", false, 1)]
        public static void Setup()
        {
            if (!FindObjectOfType<PolyTerrainMaster>()) {
                var goMaster = new GameObject("PolyTerrain Master");
                goMaster.transform.localPosition = Vector3.zero;
                goMaster.transform.localRotation = Quaternion.identity;
                goMaster.transform.localScale = Vector3.one;
                goMaster.AddComponent<PolyTerrainMaster>();
            }
            
            var terrains = FindObjectsOfType<Terrain>();
            foreach (var terrain in terrains) {
                if (!terrain.GetComponent<PolyTerrain>()) {
                    terrain.gameObject.AddComponent<PolyTerrain>();
                }
            }
            
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private void ApplySettings()
        {
            master.ApplySettings();
            SceneView.RepaintAll();
        }

        private void ApplyDraw()
        {
            var polyTerrains = FindObjectsOfType<PolyTerrain>();
            foreach (var poly in polyTerrains) {
                poly.draw = master.draw;
                poly.Update();
            }
            SceneView.RepaintAll();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This is where you can set settings of all PolyTerrain components in the scene at once.", MessageType.Info);
            
            EditorGUI.BeginChangeCheck();
            master.draw = EditorGUILayout.Toggle("Draw", master.draw);
            if (EditorGUI.EndChangeCheck()) {
                ApplyDraw();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            EditorGUI.BeginChangeCheck();
            master.style = (PolyStyle)EditorGUILayout.EnumPopup("Style", master.style);

            if (master.style == PolyStyle.Blocky) {
                EditorGUILayout.Space();
                
                if (!master.blockUVsAsset) {
                    EditorGUILayout.HelpBox("A Block UVs Asset is required so the blocky-style can be rendered. Please assign a Block UVs Asset below. " +
                                            "You can create a Block UVs Asset by using the 'Block Material Maker' below.", MessageType.Error);
                }
                master.blockUVsAsset = (BlockUVsAsset)EditorGUILayout.ObjectField("Block UVs Asset", master.blockUVsAsset, typeof(BlockUVsAsset), false);
                if (!master.GetComponent<BlockMaterialMaker>()) {
                    master.gameObject.AddComponent<BlockMaterialMaker>();
                }
                
                master.autoDisableTerrainCollider = EditorGUILayout.Toggle("Auto-Disable TerrainCollider", master.autoDisableTerrainCollider);
            }
            
            EditorGUILayout.Space();

            master.enableOcclusionCulling = EditorGUILayout.Toggle("Enable Occlusion Culling", master.enableOcclusionCulling);
            master.layer = EditorGUILayout.LayerField("Layer", master.layer);
            master.chunkSize = EditorGUILayout.IntPopup("Chunk size", master.chunkSize, new[] { "16", "32", "64" },
                new[] { 17, 33, 65 });

            if (EditorGUI.EndChangeCheck()) {
                ApplySettings();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            EditorGUILayout.Space();
            
            GUI.backgroundColor = new Color(0.47f, 1f, 0.46f);
            if (GUILayout.Button("Refresh")) {
                ApplySettings();
            }
        }
    }
}