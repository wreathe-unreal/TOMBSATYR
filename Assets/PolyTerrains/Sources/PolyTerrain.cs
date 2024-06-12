using System;
using System.Collections.Generic;
using System.Diagnostics;
using PolyTerrains.Sources.BlockMaterial;
using PolyTerrains.Sources.Polygonizers;
using Unity.Mathematics;
using UnityEngine;
#if !UNITY_2021_1_OR_NEWER
using UnityEngine.Experimental.TerrainAPI;
#endif
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolyTerrains.Sources
{
    [AddComponentMenu("PolyTerrains/Poly Terrain")]
    [ExecuteInEditMode]
    public class PolyTerrain : MonoBehaviour
    {
        [SerializeField] internal Terrain terrain;
        [SerializeField] private int chunkSize = 33;

        [SerializeField] public bool draw = true;
        [SerializeField] public PolyStyle style = PolyStyle.LowPoly;
        [SerializeField] public Material[] materials;
        [SerializeField] public MaterialPropertyBlock[] materialProperties;
        [SerializeField] public bool showDebug;
        [SerializeField] public bool enableOcclusionCulling;
        [SerializeField] public int layer = 0;
        [SerializeField] public string chunksTag = "Untagged";
        [SerializeField] public BlockUVsAsset blockUVsAsset;
        [SerializeField] public bool autoDisableTerrainCollider = true;

        [SerializeField] private bool needRefresh;
        [SerializeField] private Transform chunksParent;

        private HeightsFeeder heightsFeeder;
        private HolesFeeder holesFeeder;
        private AlphamapsFeeder alphamapsFeeder;
        private Dictionary<Vector2i, Chunk> chunks;
        private bool previousDraw;
        private PolyOut? mOut;

        internal TerrainData TerrainData => terrain.terrainData;
        internal int SizeOfMesh => chunkSize - 1;
        internal int SizeVox => chunkSize + 1;
        internal HeightsFeeder HeightsFeeder => heightsFeeder;
        internal HolesFeeder HolesFeeder => holesFeeder;
        internal AlphamapsFeeder AlphamapsFeeder => alphamapsFeeder;

        private bool IsReady => terrain != null && materials != null && materials.Length > 0 && (style != PolyStyle.Blocky || blockUVsAsset);

        public int ChunkSize {
            get => chunkSize;
            set => chunkSize = value;
        }

        internal Transform GetChunksParent()
        {
            if (chunksParent)
                return chunksParent;

            foreach (Transform child in transform) {
                if (child.name == "PolyTerrain") {
                    chunksParent = child;
                    return chunksParent;
                }
            }

            var go = new GameObject("PolyTerrain");
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            chunksParent = go.transform;
            return chunksParent;
        }

        internal PolyOut GetMeshingOut()
        {
            if (!mOut.HasValue) {
                mOut = PolyOut.New();
            }

            return mOut.Value;
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy DisposeMeshingOut");
            DisposeMeshingOut();
        }

        private void OnEnable()
        {
            Init();
            SetVisible(draw);
            TerrainCallbacks.heightmapChanged += HeightmapChangedCallback;
            TerrainCallbacks.textureChanged += TextureChangedCallback;
        }

        private void OnDisable()
        {
            TerrainCallbacks.textureChanged -= TextureChangedCallback;
            TerrainCallbacks.heightmapChanged -= HeightmapChangedCallback;
        }

        // Update is called once per frame
        public void Update()
        {
            if (!IsReady)
                return;

            if (previousDraw != draw) {
                previousDraw = draw;
                if (draw && needRefresh) {
                    needRefresh = false;
                    Generate();
                }

                SetVisible(draw);
            }

            if (draw == terrain.drawHeightmap) {
                terrain.drawHeightmap = !draw;
                UpdateTerrainCollider();
            }
        }

        public void Refresh()
        {
            if (!IsReady)
                return;
            DestroyImmediate(GetChunksParent().gameObject);
            Init();
            UpdateTerrainCollider();
        }

        private void UpdateTerrainCollider()
        {
            var terrainCollider = terrain.GetComponent<TerrainCollider>();
            if (terrainCollider && autoDisableTerrainCollider) {
                terrainCollider.enabled = !draw || style == PolyStyle.LowPoly;
            }
        }


        private void Init()
        {
            previousDraw = draw;
            terrain = GetComponent<Terrain>();
            terrain.drawInstanced = false;

            SetupMaterials();

            heightsFeeder = new HeightsFeeder(this, 1);
            holesFeeder = new HolesFeeder(this);
            alphamapsFeeder = new AlphamapsFeeder(this);

            var count = (TerrainData.heightmapResolution - 1) / SizeOfMesh;
            chunks = new Dictionary<Vector2i, Chunk>(count * count, new Vector2iComparer());

            foreach (Transform child in GetChunksParent().transform) {
                if (!child.name.StartsWith("Chunk_") || !child.GetComponent<Chunk>())
                    continue;
                var chunkPosition = Chunk.GetPositionFromName(child.name);
                var chunk = child.GetComponent<Chunk>();
                chunk.SetMaterials(materials, materialProperties);
                chunks.Add(chunkPosition, chunk);
            }


            for (var x = 0; x < count; ++x) {
                for (var z = 0; z < count; ++z) {
                    var chunkPosition = new Vector2i(x, z);
                    if (chunks.ContainsKey(chunkPosition))
                        continue;
                    var chunk = Chunk.CreateChunk(new Vector2i(x, z), this, terrain, materials, materialProperties, layer, chunksTag);
                    chunk.Build(style);
                    chunks.Add(chunkPosition, chunk);
                }
            }

            DisposeMeshingOut();
        }

        public void RefreshMaterials()
        {
            if (chunks == null)
                return;

            SetupMaterials();
            foreach (var chunk in chunks) {
                chunk.Value.SetMaterials(materials, materialProperties);
            }
        }

        private void SetupMaterials()
        {
            switch (style) {
                case PolyStyle.Blocky when blockUVsAsset:
                    materials = new[] { blockUVsAsset.material };
                    materialProperties = Array.Empty<MaterialPropertyBlock>();
                    break;
                case PolyStyle.LowPoly:
                    SetupMaterialsFromTerrain();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetupMaterialsFromTerrain()
        {
            var tData = terrain.terrainData;
            var passCount = MaterialSetup.GetPassCount(tData);

            materials = new Material[passCount];
            materialProperties = new MaterialPropertyBlock[passCount];

            for (var pass = 0; pass < passCount; ++pass) {
                materials[pass] = pass == 0 ? new Material(terrain.materialTemplate) : new Material(terrain.materialTemplate.shader.GetDependency("AddPassShader"));
                materialProperties[pass] = MaterialSetup.SetupMaterial(pass, materials[pass], terrain);
            }
        }

        private void Generate()
        {
            var watch = Stopwatch.StartNew();
            foreach (var pair in chunks) {
                heightsFeeder.Deprecate(pair.Key);
                holesFeeder.Deprecate(pair.Key);
                alphamapsFeeder.Deprecate(pair.Key);
                pair.Value.Build(style);
            }

            DisposeMeshingOut();
            watch.Stop();
            Debug.Log($"Generate done in {watch.ElapsedMilliseconds}ms");
        }

        private void DisposeMeshingOut()
        {
            mOut?.Dispose();
            mOut = null;
        }

        private void SetVisible(bool visible)
        {
            foreach (var pair in chunks) {
                pair.Value.SetVisible(visible);
            }
        }

        private void HeightmapChangedCallback(Terrain terr, RectInt heightRegion, bool synched)
        {
            if (terr != terrain)
                return;

            if (!draw) {
                needRefresh = true;
                return;
            }

            if (!synched) {
#if UNITY_EDITOR
                if (Selection.activeGameObject == terrain.gameObject) {
                    terrain.terrainData.SyncHeightmap();
                }
#endif
                return;
            }

            TerrainChangedCallback(heightRegion);
        }

        private void TextureChangedCallback(Terrain terr, string textureName, RectInt texelRegion, bool synched)
        {
            if (terr != terrain)
                return;

            if (!draw) {
                needRefresh = true;
                return;
            }

            if (!synched) {
#if UNITY_EDITOR
                if (Selection.activeGameObject == terrain.gameObject) {
                    terrain.terrainData.SyncTexture(textureName);
                }
#endif
                return;
            }

            if (textureName == TerrainData.AlphamapTextureName) {
                var terrainData = terrain.terrainData;
                var alphamapsSize = new float2(terrainData.alphamapWidth, terrainData.alphamapHeight);
                var heightmapSize = new float2(terrainData.heightmapResolution - 1, terrainData.heightmapResolution - 1);
                var coef = heightmapSize / alphamapsSize;
                texelRegion = new RectInt((int)(texelRegion.xMin * coef.x), (int)(texelRegion.yMin * coef.y), (int)(texelRegion.width * coef.x) + 1, (int)(texelRegion.height * coef.y) + 1);
            } else if (textureName == TerrainData.HolesTextureName) {
                RefreshMaterials();
            }

            TerrainChangedCallback(texelRegion);
        }

        private void TerrainChangedCallback(RectInt region)
        {
            var startX = region.x / SizeOfMesh;
            var startZ = region.y / SizeOfMesh;
            var max = (TerrainData.heightmapResolution - 1) / SizeOfMesh - 1;
            for (var x = startX; x <= math.min(startX + region.width / SizeOfMesh + 1, max); ++x) {
                for (var z = startZ; z <= math.min(startZ + region.height / SizeOfMesh + 1, max); ++z) {
                    var chunkPosition = new Vector2i(x, z);
                    heightsFeeder.Deprecate(chunkPosition);
                    holesFeeder.Deprecate(chunkPosition);
                    alphamapsFeeder.Deprecate(chunkPosition);
                    if (chunks.TryGetValue(chunkPosition, out var chunk) && IsReady) {
                        chunk.Build(style);
                    } else {
                        Debug.LogError($"Missing chunk at {chunkPosition}");
                    }
                }
            }

            DisposeMeshingOut();
        }
    }
}