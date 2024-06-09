using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
#endif

namespace PolyTerrains.Sources
{
    public class ChunkObject : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshFilter filter;
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private bool isStatic;

        internal static ChunkObject Create(
            Vector2i chunkPosition,
            Chunk chunk,
            PolyTerrain poly,
            Material[] materials,
            MaterialPropertyBlock[] materialProperties,
            int layer,
            string tag)
        {
            Utils.Profiler.BeginSample("ChunkObject.Create");
            var go = new GameObject(GetName(chunkPosition));
            go.layer = layer;
            go.tag = tag;
            go.hideFlags = poly.showDebug ? HideFlags.None : HideFlags.DontSave;

            go.transform.parent = chunk.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var chunkObject = go.AddComponent<ChunkObject>();
            chunkObject.enabled = false;
            chunkObject.meshRenderer = go.AddComponent<MeshRenderer>();
            chunkObject.meshRenderer.lightmapScaleOffset = poly.terrain.lightmapScaleOffset;
            chunkObject.meshRenderer.realtimeLightmapScaleOffset = poly.terrain.realtimeLightmapScaleOffset;
            chunkObject.SetMaterials(materials, materialProperties);

            SetupMeshRenderer(poly.terrain, chunkObject.meshRenderer);

            go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.On;
            go.GetComponent<Renderer>().receiveShadows = true;
            chunkObject.filter = go.AddComponent<MeshFilter>();
            chunkObject.meshRenderer.enabled = false;

            chunkObject.meshCollider = go.AddComponent<MeshCollider>();
            var terrainCollider = poly.terrain.GetComponent<TerrainCollider>();
            if (terrainCollider) {
                chunkObject.meshCollider.sharedMaterial = terrainCollider.sharedMaterial;
            }

            chunkObject.UpdateStaticEditorFlags(poly.enableOcclusionCulling);

            Utils.Profiler.EndSample();
            return chunkObject;
        }

        internal void SetMaterials(Material[] materials, MaterialPropertyBlock[] materialPropertyBlocks)
        {
            meshRenderer.sharedMaterials = materials ?? Array.Empty<Material>();
            if (materialPropertyBlocks != null) {
                for (var i = 0; i < materialPropertyBlocks.Length; i++) {
                    meshRenderer.SetPropertyBlock(materialPropertyBlocks[i], i);
                }
            }
        }

        public void UpdateStaticEditorFlags(bool enableOcclusionCulling)
        {
#if UNITY_EDITOR
            StaticEditorFlags flags = 0;

            isStatic = true;
#if !UNITY_2022_1_OR_NEWER
            flags = StaticEditorFlags.OffMeshLinkGeneration | StaticEditorFlags.NavigationStatic;
#endif
            if (enableOcclusionCulling)
            {
                flags |= StaticEditorFlags.OccludeeStatic |
                            StaticEditorFlags.OccluderStatic;
            }
            GameObjectUtility.SetStaticEditorFlags(gameObject, flags);
#endif
        }

        private static void SetupMeshRenderer(Terrain terrain, MeshRenderer meshRenderer)
        {
#if UNITY_EDITOR
            var terrainSerializedObject = new SerializedObject(terrain);
            var serializedObject = new SerializedObject(meshRenderer);
            var terrainLightmapParameters = terrainSerializedObject.FindProperty("m_LightmapParameters");
            var lightmapParameters = serializedObject.FindProperty("m_LightmapParameters");
            lightmapParameters.objectReferenceValue = terrainLightmapParameters.objectReferenceValue;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
#endif
        }

        public static string GetName(Vector2i chunkPosition)
        {
            return $"ChunkObject_{chunkPosition.x}_{chunkPosition.y}";
        }

        public bool PostBuild(Mesh visualMesh, Mesh collisionMesh)
        {
            Utils.Profiler.BeginSample("[Dig] Chunk.PostBuild");
            if (filter.sharedMesh && !isStatic) {
                if (Application.isEditor && !Application.isPlaying) {
                    DestroyImmediate(filter.sharedMesh, true);
                } else {
#if UNITY_EDITOR
                    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(filter.sharedMesh.GetInstanceID())))
#endif
                        Destroy(filter.sharedMesh);
                }
            }

            var hasVisualMesh = false;
            if (!ReferenceEquals(visualMesh, null) && visualMesh.vertexCount > 0) {
                filter.sharedMesh = visualMesh;
                meshRenderer.enabled = true;
                hasVisualMesh = true;
            } else {
                filter.sharedMesh = null;
                meshRenderer.enabled = false;
            }

            if (meshCollider.sharedMesh) {
                if (Application.isEditor && !Application.isPlaying) {
                    DestroyImmediate(meshCollider.sharedMesh, true);
                } else {
#if UNITY_EDITOR
                    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(meshCollider.sharedMesh.GetInstanceID())))
#endif
                        Destroy(meshCollider.sharedMesh);
                }
            }

            if (!ReferenceEquals(collisionMesh, null) && collisionMesh.vertexCount > 0) {
                meshCollider.sharedMesh = collisionMesh;
                meshCollider.enabled = true;
            } else {
                meshCollider.sharedMesh = null;
                meshCollider.enabled = false;
            }

            Utils.Profiler.EndSample();
            return hasVisualMesh;
        }

#if UNITY_EDITOR
        public void SaveMeshesAsAssets()
        {
            var sameMeshes = meshCollider && filter && meshCollider.sharedMesh == filter.sharedMesh;

            if (filter && filter.sharedMesh) {
                var mesh = EditorUtils.CreateOrReplaceAssetHard(filter.sharedMesh, Path.Combine("Assets", $"{gameObject.name}_mesh.asset"));
                filter.sharedMesh = mesh;
                if (sameMeshes) {
                    meshCollider.sharedMesh = mesh;
                }
            }

            if (meshCollider && meshCollider.sharedMesh && !sameMeshes) {
                meshCollider.sharedMesh =
                    EditorUtils.CreateOrReplaceAssetHard(meshCollider.sharedMesh, Path.Combine("Assets", $"{gameObject.name}_collisionMesh.asset"));
            }
        }
#endif
    }
}