using Digger.Modules.Core.Editor;
using Digger.Modules.Core.Sources;
using Digger.Modules.PolyTerrainsIntegration.Sources;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Digger.Modules.PolyTerrainsIntegration.Editor
{
    [OperationAttr("Set Block", 200)]
    public class SetBlockOperationEditor : IOperationEditor
    {
        private DiggerSystem[] diggerSystems;
        private readonly SetBlockOperation operation = new SetBlockOperation();

        private GameObject reticleCube;
        
        private enum OperationType
        {
            Dig,
            Add
        }

        private GameObject ReticleCube {
            get {
                if (!reticleCube) {
                    var prefab = DiggerMasterEditor.LoadAssetWithLabel(DiggerMasterEditor.GetReticleLabel("Digger_CubeReticle"));
                    reticleCube = Object.Instantiate(prefab);
                    reticleCube.hideFlags = HideFlags.HideAndDontSave;
                }

                return reticleCube;
            }
        }
        
        private int3 size {
            get => new int3(EditorPrefs.GetInt("SetBlockOperationEditor_size_x", 3), EditorPrefs.GetInt("SetBlockOperationEditor_size_y", 3), EditorPrefs.GetInt("SetBlockOperationEditor_size_z", 3));
            set {
                EditorPrefs.SetInt("SetBlockOperationEditor_size_x", value.x);
                EditorPrefs.SetInt("SetBlockOperationEditor_size_y", value.y);
                EditorPrefs.SetInt("SetBlockOperationEditor_size_z", value.z);
            }
        }
        
        private bool sizeLinked {
            get => EditorPrefs.GetBool("SetBlockOperationEditor_sizeLinked", true);
            set => EditorPrefs.SetBool("SetBlockOperationEditor_sizeLinked", value);
        }
        
        private OperationType operationType {
            get => (OperationType)EditorPrefs.GetInt("SetBlockOperationEditor_brush", (int)OperationType.Dig);
            set => EditorPrefs.SetInt("SetBlockOperationEditor_brush", (int)value);
        }
        
        private int textureIndex {
            get => EditorPrefs.GetInt("SetBlockOperationEditor_textureIndex", 0);
            set => EditorPrefs.SetInt("SetBlockOperationEditor_textureIndex", value);
        }

        private bool clicking;

        public void OnEnable()
        {
            diggerSystems = Object.FindObjectsOfType<DiggerSystem>();
        }

        public void OnDisable()
        {
            if (reticleCube)
                Object.DestroyImmediate(reticleCube);
        }

        public void OnInspectorGUI()
        {
            var diggerSystem = Object.FindObjectOfType<DiggerSystem>();
            if (!diggerSystem)
                return;
            
            EditorGUILayout.BeginVertical("Box");
            var x = EditorGUILayout.IntSlider("Size X", size.x, 1, 20);
            var y = LockableSlider("Size Y", size.y, 1, 20);
            var z = LockableSlider("Size Z", size.z, 1, 20);
            EditorGUILayout.EndVertical();
                    
            if (sizeLinked) y = size.x;
            if (sizeLinked) z = size.x;
            size = new int3(x, y, z);

            operationType = (OperationType)EditorGUILayout.EnumPopup("Dig/Add", operationType);

            textureIndex = DiggerMasterEditor.TextureSelector(textureIndex, diggerSystem);
        }
        
        private int LockableSlider(string label, int value, int left, int right)
        {
            var hRect = EditorGUILayout.BeginHorizontal();
            if (GUI.Button(new Rect(hRect.x, hRect.y + 2, 34, 34), sizeLinked ? EditorGUIUtility.IconContent("LockIcon-On") : EditorGUIUtility.IconContent("LockIcon"), GUIStyle.none)) {
                sizeLinked = !sizeLinked;
            }

            EditorGUI.BeginDisabledGroup(sizeLinked);
            var newValue = EditorGUILayout.IntSlider($"      {label}", value, left, right);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            return newValue;
        }
        
        public void OnSceneGUI()
        {
        }

        public void OnScene(UnityEditor.Editor editor, SceneView sceneview)
        {
            var e = Event.current;
            var digger = Object.FindObjectOfType<DiggerSystem>();
            if (!digger)
                return;
            
            if (!clicking && !e.alt && e.type == EventType.MouseDown && e.button == 0) {
                clicking = true;
                if (!Application.isPlaying) {
                    foreach (var diggerSystem in diggerSystems) {
                        diggerSystem.PrepareModification();
                    }
                }
            } else if (clicking && (e.type == EventType.MouseUp || e.type == EventType.MouseLeaveWindow ||
                                    (e.isKey && !e.control && !e.shift) ||
                                    e.alt || EditorWindow.mouseOverWindow == null ||
                                    EditorWindow.mouseOverWindow.GetType() != typeof(SceneView))) {
                clicking = false;
                if (!Application.isPlaying) {
                    foreach (var diggerSystem in diggerSystems) {
                        diggerSystem.PersistAndRecordUndo(false, false);
                    }
                }
            }

            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var hit = DiggerMasterEditor.GetIntersectionWithTerrainOrDigger(ray);
            if (!hit.HasValue)
                return;

            var p = new float3(hit.Value.point);
            if (operationType == OperationType.Dig) {
                p -= new float3(hit.Value.normal);
            } else {
                p += new float3(hit.Value.normal);
            }
            var vp = new int3(math.round(p / digger.HeightmapScale));
            p = Utils.VoxelToUnityPosition(vp, digger.HeightmapScale);
            UpdateReticlePosition(p, digger.HeightmapScale);

            if (clicking) {
                operation.Position = vp;
                operation.Size = size;
                operation.TextureIndex = (uint)textureIndex;
                operation.TargetValue = operationType == OperationType.Dig ? 1 : -1;

                foreach (var diggerSystem in diggerSystems) {
                    diggerSystem.Modify(operation);
                }
            }
        }

        private void UpdateReticlePosition(Vector3 position, float3 heightmapScale)
        {
            var reticle = ReticleCube.transform;
            reticle.position = position;
            reticle.localScale = Utils.VoxelToUnityPosition(size, heightmapScale) + 0.05f;
            reticle.rotation = Quaternion.identity;
        }
    }
}