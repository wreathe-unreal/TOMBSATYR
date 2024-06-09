using Unity.Collections;

namespace PolyTerrains.Sources.Polygonizers
{
    public struct PolyOut
    {
        public const int MaxVertexCount = 65536 * 4;
        public const int MaxTriangleCount = 65536 * 4;

        public NativeArray<VertexData> outVertexData;

        public NativeArray<uint> outTriangles;

        public void Dispose()
        {
            outVertexData.Dispose();
            outTriangles.Dispose();
        }

        public static PolyOut New()
        {
            return new PolyOut
            {
                outVertexData = new NativeArray<VertexData>(MaxVertexCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory),
                outTriangles = new NativeArray<uint>(MaxTriangleCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory)
            };
        }
    }
}