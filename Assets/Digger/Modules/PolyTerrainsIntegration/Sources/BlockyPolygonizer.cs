using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.Jobs;
using Digger.Modules.Core.Sources.NativeCollections;
using Digger.Modules.Core.Sources.Polygonizers;
using PolyTerrains.Sources.BlockMaterial;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Modules.PolyTerrainsIntegration.Sources
{
    public class BlockyPolygonizer : IPolygonizer
    {
        private readonly BlockUVsAsset blockUVsAsset;

        public BlockyPolygonizer(BlockUVsAsset blockUVsAsset)
        {
            this.blockUVsAsset = blockUVsAsset;
        }

        public Mesh BuildMesh(VoxelChunk chunk, int lod)
        {
            var tData = chunk.Digger.Terrain.terrainData;
            var uvScale = new Vector2(1f / tData.size.x, 1f / tData.size.z);
            var voxels = new NativeArray<Voxel>(chunk.VoxelArray, Allocator.TempJob);
            var heights = new NativeArray<float>(chunk.HeightArray, Allocator.TempJob);
            var holes = new NativeArray<int>(chunk.HolesArray, Allocator.TempJob);
            var alphamaps = new NativeArray<float>(chunk.AlphamapArray, Allocator.TempJob);

            var blockUVs = new NativeArray<BlockUV>(blockUVsAsset ? blockUVsAsset.blockUVs : new[] { BlockUV.Default }, Allocator.TempJob);

            var meshingOut = NativeCollectionsPool.Instance.GetPolyOut();
            var vertexCounter = new NativeCounter(Allocator.TempJob, 4);
            var triangleCounter = new NativeCounter(Allocator.TempJob, 6);

            var job = new BlockyPolygonizationJob()
            {
                lod = lod,
                Voxels = voxels,
                Heights = heights,
                Holes = holes,
                Alphamaps = alphamaps,
                AlphamapOrigin = chunk.AlphamapArrayOrigin,
                AlphamapsSize = new int2(tData.alphamapWidth, tData.alphamapHeight),
                LocalAlphamapsSize = chunk.AlphamapArraySize,
                BlockUVs = blockUVs,
                UVScale = uvScale,
                HeightmapScale = new float3(chunk.Digger.HeightmapScale),
                SizeVox = chunk.SizeVox,
                SizeVox2 = chunk.SizeVox * chunk.SizeVox,
                ChunkWorldPosition = chunk.WorldPosition,
                OutTriangles = meshingOut.outTriangles,
                OutVertexData = meshingOut.outVertexData,
                TriangleCounter = triangleCounter.ToConcurrent(),
                VertexCounter = vertexCounter.ToConcurrent()
            };

            // Schedule the job
            var currentJobHandle = job.Schedule(voxels.Length, 16);
            // Wait for the job to complete
            currentJobHandle.Complete();

            voxels.Dispose();
            heights.Dispose();
            holes.Dispose();
            alphamaps.Dispose();
            blockUVs.Dispose();
            var vertexCount = vertexCounter.Count;
            var triangleCount = triangleCounter.Count;
            vertexCounter.Dispose();
            triangleCounter.Dispose();

            return VoxelChunk.ToMesh(chunk, meshingOut, vertexCount, triangleCount);
        }
    }
}