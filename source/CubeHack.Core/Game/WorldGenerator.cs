// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Geometry;
using CubeHack.Randomization;
using CubeHack.State;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    public class WorldGenerator
    {
        private const uint _dirtSeed = 0xba26789cU;
        private const uint _mountainSeed = 0x512ba950U;
        private const uint _mountDetailSeed = 0xe12a62c9U;

        private static readonly Noise2D _dirtNoise = new Noise2D(_dirtSeed, 8, 16);
        private static readonly Noise2D _mountainNoise = new Noise2D(_mountainSeed, 20, 100);
        private static readonly Noise2D _mountainDetailNoise = new Noise2D(_mountDetailSeed, 7, 20);

        private readonly object _mutex = new object();
        private readonly HashSet<ChunkPos> _inProgress = new HashSet<ChunkPos>();

        public void OnChunkRequested(Chunk chunk)
        {
            lock (_mutex)
            {
                var chunkPos = chunk.Pos;
                if (chunk.IsCreated || _inProgress.Contains(chunk.Pos)) return;
                _inProgress.Add(chunkPos);

                Task.Run(
                    () =>
                    {
                        try
                        {
                            CreateChunk(chunk);
                        }
                        finally
                        {
                            lock (_mutex)
                            {
                                _inProgress.Remove(chunkPos);
                            }
                        }
                    });
            }
        }

        private void CreateChunk(Chunk chunk)
        {
            var chunkPos = chunk.Pos;

            var tempChunk = new Chunk(null, chunkPos);

            var cornerPos = (BlockPos)chunkPos;

            for (int x = 0; x < GeometryConstants.ChunkSize; ++x)
            {
                long wx = cornerPos.X + x;
                for (int z = 0; z < GeometryConstants.ChunkSize; ++z)
                {
                    long wz = cornerPos.Z + z;

                    var dirtHeight = (long)(_dirtNoise[wx, wz] * 2.5 - 8);
                    var mountainHeight = (long)(_mountainNoise[wx, wz] * 8 + _mountainDetailNoise[wx, wz] * 3 - 10);

                    for (int y = 0; y < GeometryConstants.ChunkSize; ++y)
                    {
                        long wy = cornerPos.Y + y;

                        if (wy < mountainHeight)
                        {
                            tempChunk[x, y, z] = 2; // Rock
                        }
                        else if (wy < dirtHeight)
                        {
                            tempChunk[x, y, z] = 1; // Dirt
                        }
                    }
                }
            }

            tempChunk.IsCreated = true;

            chunk.PasteChunkData(tempChunk.GetChunkData());
        }
    }
}
