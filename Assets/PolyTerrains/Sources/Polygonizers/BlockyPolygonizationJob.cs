using System;
using PolyTerrains.Sources.BlockMaterial;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace PolyTerrains.Sources.Polygonizers
{
    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
    internal struct BlockyPolygonizationJob : IJobParallelFor
    {
        public int SizeVox;
        public float3 HeightmapScale;
        public float2 ChunkWorldPosition;
        public float2 UVScale;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<float> Heights;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<byte> Holes;

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
        public NativeArray<uint> OutTriangles;


        public void Execute(int index)
        {
            var lod = 1;
            var pi0 = Utils.IndexToXZ(index, SizeVox);

            if (pi0.x >= SizeVox - lod - 1 ||
                pi0.y >= SizeVox - lod - 1 ||
                pi0.x % lod != 0 || pi0.y % lod != 0)
                return;

            if (Holes[index] == 1)
                return;

            var h = Heights[Utils.XZToHeightIndex(pi0, SizeVox)];
            var hi = (int)(h / HeightmapScale.y);

            var blockUV = GetBlockUV(pi0);
            var blockUVOnRight = GetBlockUV(pi0 + new int2(lod, 0));
            var blockUVOnForward = GetBlockUV(pi0 + new int2(0, lod));

            NewQuadTop(new float3(pi0.x, hi, pi0.y), blockUV.Top);

            var hLeft = Heights[Utils.XZToHeightIndex(pi0 + new int2(lod, 0), SizeVox)];
            var hiLeft = (int)(hLeft / HeightmapScale.y);
            var deepness = 0;
            while (hiLeft > hi) {
                NewQuadLeft(new float3(pi0.x, hiLeft, pi0.y), deepness == 0 ? blockUVOnRight.Side : blockUVOnRight.Bottom);
                hiLeft--;
                deepness++;
            }

            var hRight = hLeft;
            var hiRight = (int)(hRight / HeightmapScale.y);
            while (hiRight < hi) {
                hiRight++;
                NewQuadRight(new float3(pi0.x, hiRight, pi0.y), hiRight == hi ? blockUV.Side : blockUV.Bottom);
            }

            var hBackward = Heights[Utils.XZToHeightIndex(pi0 + new int2(0, lod), SizeVox)];
            var hiBackward = (int)(hBackward / HeightmapScale.y);
            deepness = 0;
            while (hiBackward > hi) {
                NewQuadBackward(new float3(pi0.x, hiBackward, pi0.y), deepness == 0 ? blockUVOnForward.Side : blockUVOnForward.Bottom);
                hiBackward--;
                deepness++;
            }

            var hForward = hBackward;
            var hiForward = (int)(hForward / HeightmapScale.y);
            while (hiForward < hi) {
                hiForward++;
                NewQuadForward(new float3(pi0.x, hiForward, pi0.y), hiForward == hi ? blockUV.Side : blockUV.Bottom);
            }
        }

        private BlockUV GetBlockUV(int2 pi)
        {
            var v = pi * HeightmapScale.xz;
            var uv = (ChunkWorldPosition + v) * UVScale;
            var blockUVIndex = math.min(GetHighestAlphamapAt(uv), BlockUVs.Length - 1);
            return BlockUVs[blockUVIndex];
        }

        private void NewQuadTop(float3 pi, float4 uvs)
        {
            var vIdx0 = VertexCounter.Increment() - 4;
            if (vIdx0 >= PolyOut.MaxVertexCount - 3) return;
            var v0 = NewVertex(pi, new float3(-0.5f, 0.5f, -0.5f), uvs.xy);

            var vIdx1 = vIdx0 + 1;
            var v1 = NewVertex(pi, new float3(0.5f, 0.5f, -0.5f), uvs.zy);

            var vIdx2 = vIdx1 + 1;
            var v2 = NewVertex(pi, new float3(0.5f, 0.5f, 0.5f), uvs.zw);

            var vIdx3 = vIdx2 + 1;
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
            if (triIndex >= PolyOut.MaxTriangleCount - 5) return;

            OutTriangles[triIndex + 0] = (uint)vIdx0;
            OutTriangles[triIndex + 1] = (uint)vIdx2;
            OutTriangles[triIndex + 2] = (uint)vIdx1;
            OutTriangles[triIndex + 3] = (uint)vIdx0;
            OutTriangles[triIndex + 4] = (uint)vIdx3;
            OutTriangles[triIndex + 5] = (uint)vIdx2;
        }

        private void NewQuadRight(float3 pi, float4 uvs)
        {
            var vIdx0 = VertexCounter.Increment() - 4;
            if (vIdx0 >= PolyOut.MaxVertexCount - 3) return;
            var v0 = NewVertex(pi, new float3(0.5f, -0.5f, -0.5f), uvs.xy);

            var vIdx1 = vIdx0 + 1;
            var v1 = NewVertex(pi, new float3(0.5f, -0.5f, 0.5f), uvs.zy);

            var vIdx2 = vIdx1 + 1;
            var v2 = NewVertex(pi, new float3(0.5f, 0.5f, 0.5f), uvs.zw);

            var vIdx3 = vIdx2 + 1;
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
            if (triIndex >= PolyOut.MaxTriangleCount - 5) return;

            OutTriangles[triIndex + 0] = (uint)vIdx0;
            OutTriangles[triIndex + 1] = (uint)vIdx2;
            OutTriangles[triIndex + 2] = (uint)vIdx1;
            OutTriangles[triIndex + 3] = (uint)vIdx0;
            OutTriangles[triIndex + 4] = (uint)vIdx3;
            OutTriangles[triIndex + 5] = (uint)vIdx2;
        }

        private void NewQuadLeft(float3 pi, float4 uvs)
        {
            var vIdx0 = VertexCounter.Increment() - 4;
            if (vIdx0 >= PolyOut.MaxVertexCount - 3) return;
            var v0 = NewVertex(pi, new float3(0.5f, -0.5f, -0.5f), uvs.xy);

            var vIdx1 = vIdx0 + 1;
            var v1 = NewVertex(pi, new float3(0.5f, -0.5f, 0.5f), uvs.zy);

            var vIdx2 = vIdx1 + 1;
            var v2 = NewVertex(pi, new float3(0.5f, 0.5f, 0.5f), uvs.zw);

            var vIdx3 = vIdx2 + 1;
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
            if (triIndex >= PolyOut.MaxTriangleCount - 5) return;

            OutTriangles[triIndex + 0] = (uint)vIdx0;
            OutTriangles[triIndex + 1] = (uint)vIdx1;
            OutTriangles[triIndex + 2] = (uint)vIdx2;
            OutTriangles[triIndex + 3] = (uint)vIdx0;
            OutTriangles[triIndex + 4] = (uint)vIdx2;
            OutTriangles[triIndex + 5] = (uint)vIdx3;
        }

        private void NewQuadForward(float3 pi, float4 uvs)
        {
            var vIdx0 = VertexCounter.Increment() - 4;
            if (vIdx0 >= PolyOut.MaxVertexCount - 3) return;
            var v0 = NewVertex(pi, new float3(0.5f, -0.5f, 0.5f), uvs.zy);

            var vIdx1 = vIdx0 + 1;
            var v1 = NewVertex(pi, new float3(0.5f, 0.5f, 0.5f), uvs.zw);

            var vIdx2 = vIdx1 + 1;
            var v2 = NewVertex(pi, new float3(-0.5f, 0.5f, 0.5f), uvs.xw);

            var vIdx3 = vIdx2 + 1;
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
            if (triIndex >= PolyOut.MaxTriangleCount - 5) return;

            OutTriangles[triIndex + 0] = (uint)vIdx0;
            OutTriangles[triIndex + 1] = (uint)vIdx1;
            OutTriangles[triIndex + 2] = (uint)vIdx2;
            OutTriangles[triIndex + 3] = (uint)vIdx0;
            OutTriangles[triIndex + 4] = (uint)vIdx2;
            OutTriangles[triIndex + 5] = (uint)vIdx3;
        }

        private void NewQuadBackward(float3 pi, float4 uvs)
        {
            var vIdx0 = VertexCounter.Increment() - 4;
            if (vIdx0 >= PolyOut.MaxVertexCount - 3) return;
            var v0 = NewVertex(pi, new float3(0.5f, -0.5f, 0.5f), uvs.zy);

            var vIdx1 = vIdx0 + 1;
            var v1 = NewVertex(pi, new float3(0.5f, 0.5f, 0.5f), uvs.zw);

            var vIdx2 = vIdx1 + 1;
            var v2 = NewVertex(pi, new float3(-0.5f, 0.5f, 0.5f), uvs.xw);

            var vIdx3 = vIdx2 + 1;
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
            if (triIndex >= PolyOut.MaxTriangleCount - 5) return;

            OutTriangles[triIndex + 0] = (uint)vIdx0;
            OutTriangles[triIndex + 1] = (uint)vIdx2;
            OutTriangles[triIndex + 2] = (uint)vIdx1;
            OutTriangles[triIndex + 3] = (uint)vIdx0;
            OutTriangles[triIndex + 4] = (uint)vIdx3;
            OutTriangles[triIndex + 5] = (uint)vIdx2;
        }

        private VertexData NewVertex(float3 pi, float3 relPos, float2 uv)
        {
            var v = (pi + relPos) * HeightmapScale;
            return new VertexData
            {
                Vertex = v,
                Normal = new float3(0, 1, 0),
                UV1 = uv
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