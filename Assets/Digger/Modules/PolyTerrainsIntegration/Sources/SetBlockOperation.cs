using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.Operations;
using Unity.Collections;
using Unity.Mathematics;

namespace Digger.Modules.PolyTerrainsIntegration.Sources
{
    public class SetBlockOperation : IOperation<SetBlockModificationJob>
    {
        public int3 Position;
        public uint TextureIndex;
        public int3 Size;
        public float TargetValue;

        public ModificationArea GetAreaToModify(DiggerSystem digger)
        {
            return ModificationAreaUtils.GetAABBAreaToModify(digger, Utils.VoxelToUnityPosition(Position, digger.HeightmapScale), Utils.VoxelToUnityPosition(Size, digger.HeightmapScale));
        }

        public SetBlockModificationJob Do(VoxelChunk chunk)
        {
            var job = new SetBlockModificationJob
            {
                SizeVox = chunk.SizeVox,
                SizeVox2 = chunk.SizeVox * chunk.SizeVox,
                HeightmapScale = chunk.HeightmapScale,
                ChunkAltitude = chunk.WorldPosition.y,
                Voxels = new NativeArray<Voxel>(chunk.VoxelArray, Allocator.TempJob),
                Heights = new NativeArray<float>(chunk.HeightArray, Allocator.TempJob),
                Holes = new NativeArray<int>(chunk.HolesArray, Allocator.TempJob),
                NewHolesConcurrentCounter = new NativeArray<int>(1, Allocator.TempJob),
                Center = Position - chunk.AbsoluteVoxelPosition,
                Size = Size,
                TextureIndex = TextureIndex,
                TargetValue = TargetValue,
            };
            return job;
        }

        public void Complete(SetBlockModificationJob job, VoxelChunk chunk)
        {
            job.Voxels.CopyTo(chunk.VoxelArray);
            job.Voxels.Dispose();
            job.Heights.Dispose();

            if (job.NewHolesConcurrentCounter[0] > 0) {
                chunk.Cutter.Cut(job.Holes, chunk.VoxelPosition, chunk.ChunkPosition);
            }

            job.NewHolesConcurrentCounter.Dispose();
            job.Holes.Dispose();
        }
    }
}