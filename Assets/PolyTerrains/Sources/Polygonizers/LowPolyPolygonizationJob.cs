using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace PolyTerrains.Sources.Polygonizers
{
    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
    internal struct LowPolyPolygonizationJob : IJobParallelFor
    {
        public int SizeVox;
        public float2 HeightmapScale;
        public float2 ChunkWorldPosition;
        public float2 UVScale;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<float> Heights;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<byte> Holes;

        public NativeCounter.Concurrent vertexCounter;
        public NativeCounter.Concurrent triangleCounter;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<VertexData> outVertexData;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<uint> outTriangles;


        public void Execute(int index)
        {
            var lod = 1;
            var pi0 = Utils.IndexToXZ(index, SizeVox);
            var pi1 = pi0 + new int2(lod, 0);
            var pi2 = pi0 + new int2(lod, lod);
            var pi3 = pi0 + new int2(0, lod);

            if (pi0.x >= SizeVox - lod - 1 ||
                pi0.y >= SizeVox - lod - 1 ||
                pi0.x % lod != 0 || pi0.y % lod != 0)
                return;

            if (Holes[index] == 1)
                return;

            NewTriangle(pi0, pi1, pi2);
            NewTriangle(pi0, pi2, pi3);
        }

        private void NewTriangle(int2 pi0, int2 pi1, int2 pi2)
        {
            var h0 = Heights[Utils.XZToHeightIndex(pi0, SizeVox)];
            var h1 = Heights[Utils.XZToHeightIndex(pi1, SizeVox)];
            var h2 = Heights[Utils.XZToHeightIndex(pi2, SizeVox)];

            var vIdx0 = vertexCounter.Increment() - 3;
            if (vIdx0 >= PolyOut.MaxVertexCount - 2) return;
            var v0 = NewVertex(pi0, h0);

            var vIdx1 = vIdx0 + 1;
            var v1 = NewVertex(pi1, h1);

            var vIdx2 = vIdx1 + 1;
            var v2 = NewVertex(pi2, h2);

            v0.Normal = math.normalize(math.cross(v2.Vertex - v0.Vertex, v1.Vertex - v0.Vertex));
            v1.Normal = v0.Normal;
            v2.Normal = v0.Normal;

            outVertexData[vIdx0] = v0;
            outVertexData[vIdx1] = v1;
            outVertexData[vIdx2] = v2;

            var triIndex = triangleCounter.Increment() - 3;
            if (triIndex >= PolyOut.MaxTriangleCount - 2) return;

            outTriangles[triIndex + 0] = (uint)vIdx0;
            outTriangles[triIndex + 1] = (uint)vIdx2;
            outTriangles[triIndex + 2] = (uint)vIdx1;
        }

        private VertexData NewVertex(int2 pi, float height)
        {
            var p = pi * HeightmapScale;
            var v = new float3(p.x, height, p.y);
            return new VertexData
            {
                Vertex = v,
                Normal = new float3(0, 1, 0),
                UV1 = new float2((ChunkWorldPosition.x + v.x) * UVScale.x, (ChunkWorldPosition.y + v.z) * UVScale.y)
            };
        }
    }
}