using System;
using System.Globalization;
using PolyTerrains.Sources.BlockMaterial;
using PolyTerrains.Sources.Polygonizers;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace PolyTerrains.Sources
{
    public class Chunk : MonoBehaviour
    {
        private const MeshUpdateFlags MeshUpdateFlags = UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds | UnityEngine.Rendering.MeshUpdateFlags.DontValidateIndices | UnityEngine.Rendering.MeshUpdateFlags.DontNotifyMeshUsers |
                                                        UnityEngine.Rendering.MeshUpdateFlags.DontResetBoneBounds;

        [SerializeField] private PolyTerrain poly;
        [SerializeField] private Vector2i chunkPosition;
        [SerializeField] private ChunkObject chunkObject;
        [SerializeField] private Vector2i voxelPosition;
        [SerializeField] private float2 worldPosition;

        internal static Chunk CreateChunk(Vector2i chunkPosition,
            PolyTerrain poly,
            Terrain terrain,
            Material[] materials,
            MaterialPropertyBlock[] materialProperties,
            int layer,
            string tag)
        {
            Utils.Profiler.BeginSample("CreateChunk");
            var tData = poly.TerrainData;
            var voxelPosition = GetVoxelPosition(poly, chunkPosition);
            var heightmapScale = new float2(tData.heightmapScale.x, tData.heightmapScale.z);
            var worldPosition = voxelPosition * heightmapScale;

            var go = new GameObject(GetName(chunkPosition));
            go.layer = layer;
            go.hideFlags = HideFlags.DontSave;

            go.transform.parent = poly.GetChunksParent();
            go.transform.localPosition = new Vector3(worldPosition.x, 0, worldPosition.y);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var chunk = go.AddComponent<Chunk>();
            chunk.poly = poly;
            chunk.chunkPosition = chunkPosition;
            chunk.voxelPosition = new Vector2i(voxelPosition.x, voxelPosition.y);
            chunk.worldPosition = worldPosition;
            chunk.chunkObject = ChunkObject.Create(chunkPosition, chunk, poly, materials, materialProperties, layer, tag);

            Utils.Profiler.EndSample();
            return chunk;
        }

        public static string GetName(Vector2i chunkPosition)
        {
            return $"Chunk_{chunkPosition.x}_{chunkPosition.y}";
        }

        public static Vector2i GetPositionFromName(string chunkName)
        {
            var coords = chunkName.Replace("Chunk_", "").Split('_');
            return new Vector2i(int.Parse(coords[0], CultureInfo.InvariantCulture), int.Parse(coords[1], CultureInfo.InvariantCulture));
        }

        internal void Build(PolyStyle style)
        {
            switch (style) {
                case PolyStyle.LowPoly:
                    BuildLowPoly();
                    break;
                case PolyStyle.Blocky:
                    BuildBlocky();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }
        }

        private void BuildLowPoly()
        {
            var heightArray = poly.HeightsFeeder.GetHeights(chunkPosition, voxelPosition);
            var holesArray = poly.HolesFeeder.GetHoles(chunkPosition, voxelPosition);
            var tData = poly.TerrainData;
            var uvScale = new Vector2(1f / tData.size.x, 1f / tData.size.z);
            var heights = new NativeArray<float>(heightArray, Allocator.TempJob);
            var holes = new NativeArray<byte>(holesArray, Allocator.TempJob);

            var meshingOut = poly.GetMeshingOut();
            var vertexCounter = new NativeCounter(Allocator.TempJob, 3);
            var triangleCounter = new NativeCounter(Allocator.TempJob, 3);

            var job = new LowPolyPolygonizationJob()
            {
                Heights = heights,
                Holes = holes,
                UVScale = uvScale,
                HeightmapScale = new float2(tData.heightmapScale.x, tData.heightmapScale.z),
                SizeVox = poly.SizeVox,
                ChunkWorldPosition = worldPosition,
                outTriangles = meshingOut.outTriangles,
                outVertexData = meshingOut.outVertexData,
                triangleCounter = triangleCounter.ToConcurrent(),
                vertexCounter = vertexCounter.ToConcurrent()
            };

            // Schedule the job
            var currentJobHandle = job.Schedule(poly.SizeVox * poly.SizeVox, 16);
            // Wait for the job to complete
            currentJobHandle.Complete();

            heights.Dispose();
            holes.Dispose();
            var vertexCount = vertexCounter.Count;
            var triangleCount = triangleCounter.Count;
            vertexCounter.Dispose();
            triangleCounter.Dispose();

            var mesh = ToMesh(meshingOut, vertexCount, triangleCount);
            ApplyMesh(mesh);
        }

        private void BuildBlocky()
        {
            var heightArray = poly.HeightsFeeder.GetHeights(chunkPosition, voxelPosition);
            var holesArray = poly.HolesFeeder.GetHoles(chunkPosition, voxelPosition);
            var alphamapsInfo = poly.AlphamapsFeeder.GetAlphamaps(chunkPosition, worldPosition, poly.SizeOfMesh);
            var tData = poly.TerrainData;
            var uvScale = new Vector2(1f / tData.size.x, 1f / tData.size.z);
            var heights = new NativeArray<float>(heightArray, Allocator.TempJob);
            var holes = new NativeArray<byte>(holesArray, Allocator.TempJob);
            var alphamaps = new NativeArray<float>(alphamapsInfo.AlphamapArray, Allocator.TempJob);
            var blockUVs = new NativeArray<BlockUV>(poly.blockUVsAsset.blockUVs, Allocator.TempJob);

            var meshingOut = poly.GetMeshingOut();
            var vertexCounter = new NativeCounter(Allocator.TempJob, 4);
            var triangleCounter = new NativeCounter(Allocator.TempJob, 6);

            var job = new BlockyPolygonizationJob()
            {
                Heights = heights,
                Holes = holes,
                Alphamaps = alphamaps,
                AlphamapOrigin = alphamapsInfo.AlphamapArrayOrigin,
                AlphamapsSize = new int2(tData.alphamapWidth, tData.alphamapHeight),
                LocalAlphamapsSize = alphamapsInfo.AlphamapArraySize,
                BlockUVs = blockUVs,
                UVScale = uvScale,
                HeightmapScale = new float3(tData.heightmapScale.x, tData.heightmapScale.x, tData.heightmapScale.z),
                SizeVox = poly.SizeVox,
                ChunkWorldPosition = worldPosition,
                OutTriangles = meshingOut.outTriangles,
                OutVertexData = meshingOut.outVertexData,
                TriangleCounter = triangleCounter.ToConcurrent(),
                VertexCounter = vertexCounter.ToConcurrent()
            };

            // Schedule the job
            var currentJobHandle = job.Schedule(poly.SizeVox * poly.SizeVox, 16);
            // Wait for the job to complete
            currentJobHandle.Complete();

            heights.Dispose();
            holes.Dispose();
            alphamaps.Dispose();
            blockUVs.Dispose();
            var vertexCount = vertexCounter.Count;
            var triangleCount = triangleCounter.Count;
            vertexCounter.Dispose();
            triangleCounter.Dispose();

            var mesh = ToMesh(meshingOut, vertexCount, triangleCount);
            ApplyMesh(mesh);
        }

        private void ApplyMesh(Mesh mesh)
        {
            chunkObject.PostBuild(mesh, poly.style == PolyStyle.LowPoly ? null : mesh);
        }

        private Mesh ToMesh(PolyOut o, int vertexCount, int triangleCount)
        {
            if (vertexCount < 3 || triangleCount < 1 || vertexCount >= PolyOut.MaxVertexCount || triangleCount >= PolyOut.MaxTriangleCount)
                return null;

            var mesh = new Mesh();
            AddVertexData(mesh, o, vertexCount, triangleCount);

            mesh.RecalculateBounds();
            return mesh;
        }

        private void AddVertexData(Mesh mesh, PolyOut o, int vertexCount, int triangleCount)
        {
            mesh.SetVertexBufferParams(vertexCount, VertexData.Layout);
            mesh.SetVertexBufferData(o.outVertexData, 0, 0, vertexCount, 0, MeshUpdateFlags);
            mesh.SetIndexBufferParams(triangleCount, IndexFormat.UInt32);
            mesh.SetIndexBufferData(o.outTriangles, 0, 0, triangleCount, MeshUpdateFlags);
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, triangleCount), MeshUpdateFlags);
        }

        internal void SetVisible(bool visible)
        {
            chunkObject.gameObject.SetActive(visible);
        }

        internal void SetMaterials(Material[] mat, MaterialPropertyBlock[] materialPropertyBlocks)
        {
            chunkObject.SetMaterials(mat, materialPropertyBlocks);
        }

        private static int2 GetVoxelPosition(PolyTerrain poly, Vector2i chunkPosition)
        {
            var p = chunkPosition * poly.SizeOfMesh;
            return new int2(p.x, p.y);
        }
    }
}