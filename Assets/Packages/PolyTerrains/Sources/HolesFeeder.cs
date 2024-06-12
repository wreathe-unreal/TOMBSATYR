using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace PolyTerrains.Sources
{
    public class HolesFeeder
    {
        private readonly Dictionary<Vector2i, byte[]> holesPerChunk = new Dictionary<Vector2i, byte[]>(new Vector2iComparer());
        private readonly PolyTerrain poly;
        private readonly TerrainData terrainData;

        public HolesFeeder(PolyTerrain poly)
        {
            this.poly = poly;
            this.terrainData = poly.TerrainData;
        }

        public void Deprecate(Vector2i chunkPosition)
        {
            holesPerChunk.Remove(chunkPosition);
        }

        public byte[] GetHoles(Vector2i chunkPosition, Vector2i chunkVoxelPosition)
        {
            var cpos = new Vector2i(chunkPosition.x, chunkPosition.y);
            if (holesPerChunk.TryGetValue(cpos, out var holesArray)) {
                return holesArray;
            }

            var size = poly.SizeVox; // we take heights more then chunk size
            holesArray = new byte[size * size];

            var holes = terrainData.GetHoles(chunkVoxelPosition.x, chunkVoxelPosition.y, math.min(size, terrainData.holesResolution - chunkVoxelPosition.x), math.min(size, terrainData.holesResolution - chunkVoxelPosition.y));
            for (var xi = 0; xi < size; ++xi) {
                for (var zi = 0; zi < size; ++zi) {
                    var safeX = math.min(xi, holes.GetLength(1) - 1);
                    var safeZ = math.min(zi, holes.GetLength(0) - 1);
                    holesArray[xi * size + zi] = (byte)(holes[safeZ, safeX] ? 0 : 1);
                }
            }

            holesPerChunk.Add(cpos, holesArray);
            return holesArray;
        }
    }
}