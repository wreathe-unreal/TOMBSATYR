using System;
using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.NativeCollections;
using PolyTerrains.Sources.BlockMaterial;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Utils = Digger.Modules.Core.Sources.Utils;
using VertexData = Digger.Modules.Core.Sources.VertexData;

namespace Digger.Modules.PolyTerrainsIntegration.Sources
{
    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
    internal struct BlockyPolygonizationJob : IJobParallelFor
    {
        private const int MaxVertexCount = 65536;
        private const int MaxTriangleCount = 65536;


        public int SizeVox;
        public int SizeVox2;
        public float3 HeightmapScale;
        public float3 ChunkWorldPosition;
        public float2 UVScale;

        public int lod;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<Voxel> Voxels;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<float> Heights;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> Holes;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<BlockUV> BlockUVs;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<float> Alphamaps;

        public int2 AlphamapsSize;
        public int3 LocalAlphamapsSize;
        public int2 AlphamapOrigin;

        public NativeCounter.Concurrent VertexCounter;
        public NativeCounter.Concurrent TriangleCounter;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<VertexData> OutVertexData;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<ushort> OutTriangles;


        public void Execute(int index)
        {
            var pi = Utils.IndexToXYZ(index, SizeVox, SizeVox2);

            if (pi.x >= SizeVox - lod - 1 ||
                pi.y >= SizeVox - lod - 1 ||
                pi.z >= SizeVox - lod - 1 ||
                pi.x % lod != 0 || pi.y % lod != 0 || pi.z % lod != 0)
                return;

            var v0 = Voxels[pi.x * SizeVox * SizeVox + pi.y * SizeVox + pi.z];
            var v1 = Voxels[(pi.x + lod) * SizeVox * SizeVox + pi.y * SizeVox + pi.z];
            var v3 = Voxels[pi.x * SizeVox * SizeVox + pi.y * SizeVox + (pi.z + lod)];
            var v4 = Voxels[pi.x * SizeVox * SizeVox + (pi.y + lod) * SizeVox + pi.z];

            var alt0 = v0.Alteration;
            var alt1 = v1.Alteration;
            var alt3 = v3.Alteration;
            var alt4 = v4.Alteration;
            if (alt0 == Voxel.Hole ||
                alt1 == Voxel.Hole ||
                alt3 == Voxel.Hole ||
                alt4 == Voxel.Hole)
                return;

            if (alt0 == Voxel.Unaltered &&
                alt1 == Voxel.Unaltered &&
                alt3 == Voxel.Unaltered &&
                alt4 == Voxel.Unaltered)
                return;


            if (v0.IsInsideInclusive && !v4.IsInsideInclusive) {
                var blockUV = GetBlockUV(v0, pi);
                NewQuadTop(pi, blockUV.Top);
            } else if (!v0.IsInsideInclusive && v4.IsInsideInclusive) {
                var blockUV = GetBlockUV(v4, pi);
                NewQuadBottom(pi, blockUV.Bottom);
            }

            if (v0.IsInsideInclusive && !v1.IsInsideInclusive) {
                var blockUV = GetBlockUV(v0, pi);
                NewQuadRight(pi, v4.IsInsideInclusive ? blockUV.Bottom : blockUV.Side);
            } else if (!v0.IsInsideInclusive && v1.IsInsideInclusive) {
                var v5 = Voxels[(pi.x + lod) * SizeVox * SizeVox + (pi.y + lod) * SizeVox + pi.z];
                var blockUV = GetBlockUV(v1, pi);
                NewQuadLeft(pi, v5.IsInsideInclusive ? blockUV.Bottom : blockUV.Side);
            }

            if (v0.IsInsideInclusive && !v3.IsInsideInclusive) {
                var blockUV = GetBlockUV(v0, pi);
                NewQuadForward(pi, v4.IsInsideInclusive ? blockUV.Bottom : blockUV.Side);
            } else if (!v0.IsInsideInclusive && v3.IsInsideInclusive) {
                var v7 = Voxels[pi.x * SizeVox * SizeVox + (pi.y + lod) * SizeVox + pi.z + lod];
                var blockUV = GetBlockUV(v3, pi);
                NewQuadBackward(pi, v7.IsInsideInclusive ? blockUV.Bottom : blockUV.Side);
            }
        }

        private BlockUV GetBlockUV(Voxel voxel, int3 pi)
        {
            int blockUVIndex;
            var alt = voxel.Alteration;
            if (alt == Voxel.Unaltered || alt == Voxel.OnSurface) {
                var v = pi.xz * HeightmapScale.xz;
                var uv = (ChunkWorldPosition.xz + v) * UVScale;
                blockUVIndex = math.min(GetHighestAlphamapAt(uv), BlockUVs.Length - 1);
            } else {
                blockUVIndex = math.min(voxel.NormalizedTextureLerp <= 0.5f ? (int)voxel.FirstTextureIndex : (int)voxel.SecondTextureIndex, BlockUVs.Length - 1);
            }

            return BlockUVs[blockUVIndex];
        }

        private void NewQuadTop(float3 pi, float4 uvs)
        {
            var vIdx0 = (ushort)(VertexCounter.Increment() - 4);
            if (vIdx0 >= MaxVertexCount - 3) return;
            var v0 = NewVertex(pi, new float3(-0.5f, 0.5f, -0.5f), uvs.xy);

            var vIdx1 = (ushort)(vIdx0 + 1);
            var v1 = NewVertex(pi, new float3(0.5f, 0.5f, -0.5f), uvs.zy);

            var vIdx2 = (ushort)(vIdx1 + 1);
            var v2 = NewVertex(pi, new float3(0.5f, 0.5f, 0.5f), uvs.zw);

            var vIdx3 = (ushort)(vIdx2 + 1);
            var v3 = NewVertex(pi, new float3(-0.5f, 0.5f, 0.5f), uvs.xw);

            v0.Normal = new float3(0, 1, 0);
            v1.Normal = v0.Normal;
            v2.Normal = v0.Normal;
            v3.Normal = v0.Normal;

            OutVertexData[vIdx0] = v0;
            OutVertexData[vIdx1] = v1;
            OutVertexData[vIdx2] = v2;
            OutVertexData[vIdx3] = v3;

            var triIndex = TriangleCounter.Increment() - 6;
            if (triIndex >= MaxTriangleCount - 5) return;

            OutTriangles[triIndex + 0] = vIdx0;
            OutTriangles[triIndex + 1] = vIdx2;
            OutTriangles[triIndex + 2] = vIdx1;
            OutTriangles[triIndex + 3] = vIdx0;
            OutTriangles[triIndex + 4] = vIdx3;
            OutTriangles[triIndex + 5] = vIdx2;
        }

        private void NewQuadBottom(float3 pi, float4 uvs)
        {
            var vIdx0 = (ushort)(VertexCounter.Increment() - 4);
            if (vIdx0 >= MaxVertexCount - 3) return;
            var v0 = NewVertex(pi, new float3(-0.5f, 0.5f, -0.5f), uvs.xy);

            var vIdx1 = (ushort)(vIdx0 + 1);
            var v1 = NewVertex(pi, new float3(0.5f, 0.5f, -0.5f), uvs.zy);

            var vIdx2 = (ushort)(vIdx1 + 1);
            var v2 = NewVertex(pi, new float3(0.5f, 0.5f, 0.5f), uvs.zw);

            var vIdx3 = (ushort)(vIdx2 + 1);
            var v3 = NewVertex(pi, new float3(-0.5f, 0.5f, 0.5f), uvs.xw);

            v0.Normal = new float3(0, -1, 0);
            v1.Normal = v0.Normal;
            v2.Normal = v0.Normal;
            v3.Normal = v0.Normal;

            OutVertexData[vIdx0] = v0;
            OutVertexData[vIdx1] = v1;
            OutVertexData[vIdx2] = v2;
            OutVertexData[vIdx3] = v3;

            var triIndex = TriangleCounter.Increment() - 6;
            if (triIndex >= MaxTriangleCount - 5) return;

            OutTriangles[triIndex + 0] = vIdx0;
            OutTriangles[triIndex + 1] = vIdx1;
            OutTriangles[triIndex + 2] = vIdx2;
            OutTriangles[triIndex + 3] = vIdx0;
            OutTriangles[triIndex + 4] = vIdx2;
            OutTriangles[triIndex + 5] = vIdx3;
        }

        private void NewQuadRight(float3 pi, float4 uvs)
        {
            var vIdx0 = (ushort)(VertexCounter.Increment() - 4);
            if (vIdx0 >= MaxVertexCount - 3) return;
            var v0 = NewVertex(pi, new float3(0.5f, -0.5f, -0.5f), uvs.xy);

            var vIdx1 = (ushort)(vIdx0 + 1);
            var v1 = NewVertex(pi, new float3(0.5f, -0.5f, 0.5f), uvs.zy);

            var vIdx2 = (ushort)(vIdx1 + 1);
            var v2 = NewVertex(pi, new float3(0.5f, 0.5f, 0.5f), uvs.zw);

            var vIdx3 = (ushort)(vIdx2 + 1);
            var v3 = NewVertex(pi, new float3(0.5f, 0.5f, -0.5f), uvs.xw);

            v0.Normal = new float3(1, 0, 0);
            v1.Normal = v0.Normal;
            v2.Normal = v0.Normal;
            v3.Normal = v0.Normal;

            OutVertexData[vIdx0] = v0;
            OutVertexData[vIdx1] = v1;
            OutVertexData[vIdx2] = v2;
            OutVertexData[vIdx3] = v3;

            var triIndex = TriangleCounter.Increment() - 6;
            if (triIndex >= MaxTriangleCount - 5) return;

            OutTriangles[triIndex + 0] = vIdx0;
            OutTriangles[triIndex + 1] = vIdx2;
            OutTriangles[triIndex + 2] = vIdx1;
            OutTriangles[triIndex + 3] = vIdx0;
            OutTriangles[triIndex + 4] = vIdx3;
            OutTriangles[triIndex + 5] = vIdx2;
        }

        private void NewQuadLeft(float3 pi, float4 uvs)
        {
            var vIdx0 = (ushort)(VertexCounter.Increment() - 4);
            if (vIdx0 >= MaxVertexCount - 3) return;
            var v0 = NewVertex(pi, new float3(0.5f, -0.5f, -0.5f), uvs.xy);

            var vIdx1 = (ushort)(vIdx0 + 1);
            var v1 = NewVertex(pi, new float3(0.5f, -0.5f, 0.5f), uvs.zy);

            var vIdx2 = (ushort)(vIdx1 + 1);
            var v2 = NewVertex(pi, new float3(0.5f, 0.5f, 0.5f), uvs.zw);

            var vIdx3 = (ushort)(vIdx2 + 1);
            var v3 = NewVertex(pi, new float3(0.5f, 0.5f, -0.5f), uvs.xw);

            v0.Normal = new float3(-1, 0, 0);
            v1.Normal = v0.Normal;
            v2.Normal = v0.Normal;
            v3.Normal = v0.Normal;

            OutVertexData[vIdx0] = v0;
            OutVertexData[vIdx1] = v1;
            OutVertexData[vIdx2] = v2;
            OutVertexData[vIdx3] = v3;

            var triIndex = TriangleCounter.Increment() - 6;
            if (triIndex >= MaxTriangleCount - 5) return;

            OutTriangles[triIndex + 0] = vIdx0;
            OutTriangles[triIndex + 1] = vIdx1;
            OutTriangles[triIndex + 2] = vIdx2;
            OutTriangles[triIndex + 3] = vIdx0;
            OutTriangles[triIndex + 4] = vIdx2;
            OutTriangles[triIndex + 5] = vIdx3;
        }

        private void NewQuadForward(float3 pi, float4 uvs)
        {
            var vIdx0 = (ushort)(VertexCounter.Increment() - 4);
            if (vIdx0 >= MaxVertexCount - 3) return;
            var v0 = NewVertex(pi, new float3(0.5f, -0.5f, 0.5f), uvs.zy);

            var vIdx1 = (ushort)(vIdx0 + 1);
            var v1 = NewVertex(pi, new float3(0.5f, 0.5f, 0.5f), uvs.zw);

            var vIdx2 = (ushort)(vIdx1 + 1);
            var v2 = NewVertex(pi, new float3(-0.5f, 0.5f, 0.5f), uvs.xw);

            var vIdx3 = (ushort)(vIdx2 + 1);
            var v3 = NewVertex(pi, new float3(-0.5f, -0.5f, 0.5f), uvs.xy);

            v0.Normal = new float3(0, 0, 1);
            v1.Normal = v0.Normal;
            v2.Normal = v0.Normal;
            v3.Normal = v0.Normal;

            OutVertexData[vIdx0] = v0;
            OutVertexData[vIdx1] = v1;
            OutVertexData[vIdx2] = v2;
            OutVertexData[vIdx3] = v3;

            var triIndex = TriangleCounter.Increment() - 6;
            if (triIndex >= MaxTriangleCount - 5) return;

            OutTriangles[triIndex + 0] = vIdx0;
            OutTriangles[triIndex + 1] = vIdx1;
            OutTriangles[triIndex + 2] = vIdx2;
            OutTriangles[triIndex + 3] = vIdx0;
            OutTriangles[triIndex + 4] = vIdx2;
            OutTriangles[triIndex + 5] = vIdx3;
        }

        private void NewQuadBackward(float3 pi, float4 uvs)
        {
            var vIdx0 = (ushort)(VertexCounter.Increment() - 4);
            if (vIdx0 >= MaxVertexCount - 3) return;
            var v0 = NewVertex(pi, new float3(0.5f, -0.5f, 0.5f), uvs.zy);

            var vIdx1 = (ushort)(vIdx0 + 1);
            var v1 = NewVertex(pi, new float3(0.5f, 0.5f, 0.5f), uvs.zw);

            var vIdx2 = (ushort)(vIdx1 + 1);
            var v2 = NewVertex(pi, new float3(-0.5f, 0.5f, 0.5f), uvs.xw);

            var vIdx3 = (ushort)(vIdx2 + 1);
            var v3 = NewVertex(pi, new float3(-0.5f, -0.5f, 0.5f), uvs.xy);

            v0.Normal = new float3(0, 0, -1);
            v1.Normal = v0.Normal;
            v2.Normal = v0.Normal;
            v3.Normal = v0.Normal;

            OutVertexData[vIdx0] = v0;
            OutVertexData[vIdx1] = v1;
            OutVertexData[vIdx2] = v2;
            OutVertexData[vIdx3] = v3;

            var triIndex = TriangleCounter.Increment() - 6;
            if (triIndex >= MaxTriangleCount - 5) return;

            OutTriangles[triIndex + 0] = vIdx0;
            OutTriangles[triIndex + 1] = vIdx2;
            OutTriangles[triIndex + 2] = vIdx1;
            OutTriangles[triIndex + 3] = vIdx0;
            OutTriangles[triIndex + 4] = vIdx3;
            OutTriangles[triIndex + 5] = vIdx2;
        }

        private VertexData NewVertex(float3 pi, float3 relPos, float2 uv)
        {
            var v = (pi + relPos) * HeightmapScale;
            return new VertexData
            {
                Vertex = v,
                Normal = new float3(0, 1, 0),
                UV = uv
            };
        }

        private int GetHighestAlphamapAt(float2 uv)
        {
            // adjust splatUVs so the edges of the terrain tile lie on pixel centers
            var splatUV = new float2(uv.x * (AlphamapsSize.x - 1), uv.y * (AlphamapsSize.y - 1));

            var wx = math.clamp(Convert.ToInt32(math.floor(splatUV.x)), 0, AlphamapsSize.x - 2);
            var wz = math.clamp(Convert.ToInt32(math.floor(splatUV.y)), 0, AlphamapsSize.y - 2);
            var x = math.clamp(wx - AlphamapOrigin.x, 0, LocalAlphamapsSize.x - 2);
            var z = math.clamp(wz - AlphamapOrigin.y, 0, LocalAlphamapsSize.y - 2);

            var mapCount = LocalAlphamapsSize.z;
            var max = float.NegativeInfinity;
            var result = 0;
            for (var index = 0; index < mapCount; index++) {
                var ctrl = GetAlphamap(index, mapCount, x, z, 0);
                if (ctrl > max) {
                    max = ctrl;
                    result = index;
                }
            }

            return result;
        }

        private float GetAlphamap(int index, int mapCount, int x, int z, int i)
        {
            return Alphamaps[x * LocalAlphamapsSize.y * mapCount + z * mapCount + index + i];
        }
    }
}