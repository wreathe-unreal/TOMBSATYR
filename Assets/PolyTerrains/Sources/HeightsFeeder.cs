using System.Collections.Generic;
using UnityEngine;

namespace PolyTerrains.Sources
{
    public class HeightsFeeder
    {
        private readonly Dictionary<Vector2i, float[]> heightsPerChunk = new Dictionary<Vector2i, float[]>(new Vector2iComparer());
        private readonly PolyTerrain poly;
        private readonly TerrainData terrainData;
        private readonly int resolution;
        private readonly float resolutionInv;

        public HeightsFeeder(PolyTerrain poly, int resolution)
        {
            this.poly = poly;
            this.terrainData = poly.TerrainData;
            this.resolution = resolution;
            this.resolutionInv = 1f / resolution;
        }

        public void Deprecate(Vector2i chunkPosition)
        {
            heightsPerChunk.Remove(chunkPosition);
        }

        public float GetHeight(int x, int z)
        {
            if (resolution == 1)
                return terrainData.GetHeight(x, z);

            var xr = x / resolution;
            var zr = z / resolution;
            return Utils.BilinearInterpolate(terrainData.GetHeight(xr, zr),
                terrainData.GetHeight(xr, zr + 1),
                terrainData.GetHeight(xr + 1, zr),
                terrainData.GetHeight(xr + 1, zr + 1),
                x % resolution * resolutionInv,
                z % resolution * resolutionInv);
        }

        public float[] GetHeights(Vector2i chunkPosition, Vector2i chunkVoxelPosition)
        {
            var cpos = new Vector2i(chunkPosition.x, chunkPosition.y);
            if (heightsPerChunk.TryGetValue(cpos, out var heightArray)) {
                return heightArray;
            }

            Utils.Profiler.BeginSample("VoxelChunk.FeedHeights");
            var size = poly.SizeVox + 2; // we take heights more then chunk size
            heightArray = new float[size * size];

            Utils.Profiler.BeginSample("VoxelChunk.FeedHeights>Loop");
            for (var xi = 0; xi < size; ++xi) {
                for (var zi = 0; zi < size; ++zi) {
                    heightArray[xi * size + zi] = GetHeight(chunkVoxelPosition.x + xi - 1, chunkVoxelPosition.y + zi - 1);
                }
            }

            Utils.Profiler.EndSample();
            Utils.Profiler.EndSample();
            heightsPerChunk.Add(cpos, heightArray);
            return heightArray;
        }
    }
}