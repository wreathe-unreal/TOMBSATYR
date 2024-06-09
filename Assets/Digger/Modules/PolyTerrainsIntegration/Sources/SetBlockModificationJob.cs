using Digger.Modules.Core.Sources;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Digger.Modules.PolyTerrainsIntegration.Sources
{
    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast)]
    public struct SetBlockModificationJob : IJobParallelFor
    {
        public int SizeVox;
        public int SizeVox2;
        public int3 Center;
        public int3 Size;
        public float TargetValue;
        public uint TextureIndex;
        public float3 HeightmapScale;
        public float ChunkAltitude;


        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeArray<float> Heights;

        public NativeArray<Voxel> Voxels;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> Holes;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> NewHolesConcurrentCounter;

        public void Execute(int index)
        {
            var pi = Utils.IndexToXYZ(index, SizeVox, SizeVox2);
            var dist = pi - Center;

            if (dist.x >= Size.x || dist.x <= -Size.x ||
                dist.y >= Size.y || dist.y <= -Size.y ||
                dist.z >= Size.z || dist.z <= -Size.z) {
                return;
            }

            var voxel = Voxels[index];
            voxel.Value = TargetValue;
            voxel.Alteration = Voxel.FarAboveSurface;
            voxel.SetTexture(TextureIndex, 1f);

            var p = pi * HeightmapScale;
            var terrainHeight = Heights[Utils.XYZToHeightIndex(pi, SizeVox)];
            var terrainHeightValue = p.y + ChunkAltitude - terrainHeight;
            voxel = Utils.AdjustAlteration(voxel, pi, HeightmapScale.y, p.y + ChunkAltitude, terrainHeightValue, SizeVox, Heights);

            if (voxel.IsAlteredNearBelowSurface || voxel.IsAlteredNearAboveSurface) {
                Core.Sources.NativeCollections.Utils.IncrementAt(NewHolesConcurrentCounter, 0);
                Core.Sources.NativeCollections.Utils.IncrementAt(Holes, Utils.XZToHoleIndex(pi.x, pi.z, SizeVox));
                if (pi.x >= 1) {
                    Core.Sources.NativeCollections.Utils.IncrementAt(Holes, Utils.XZToHoleIndex(pi.x - 1, pi.z, SizeVox));
                    if (pi.z >= 1) {
                        Core.Sources.NativeCollections.Utils.IncrementAt(Holes, Utils.XZToHoleIndex(pi.x - 1, pi.z - 1, SizeVox));
                    }
                }

                if (pi.z >= 1) {
                    Core.Sources.NativeCollections.Utils.IncrementAt(Holes, Utils.XZToHoleIndex(pi.x, pi.z - 1, SizeVox));
                }
            }

            Voxels[index] = voxel;
        }
    }
}