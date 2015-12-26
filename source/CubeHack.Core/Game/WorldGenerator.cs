// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Geometry;
using System;

namespace CubeHack.Game
{
    public class WorldGenerator
    {
        private readonly World _world;

        public WorldGenerator(World world)
        {
            _world = world;
        }

        public void CreateChunk(Chunk chunk)
        {
            for (int x = 0; x < GeometryConstants.ChunkSize; ++x)
            {
                long wx = ((long)chunk.Pos.X << GeometryConstants.ChunkSizeBits) + x;
                for (int z = 0; z < GeometryConstants.ChunkSize; ++z)
                {
                    long wz = ((long)chunk.Pos.Z << GeometryConstants.ChunkSizeBits) + z;

                    long y1 = (long)(8 * (Math.Sin(wx * 0.08 + 1) + Math.Sin(wz * 0.073 + 2))) - 16;
                    long y2 = (long)(3 * (Math.Sin(wx * 0.23 + wz * 0.02 + 1) + Math.Sin(-wx * 0.05 + wz * 0.31 + 2))) - 12;

                    for (int y = 0; y < GeometryConstants.ChunkSize; ++y)
                    {
                        long wy = ((long)chunk.Pos.Y << GeometryConstants.ChunkSizeBits) + y;

                        if (wy < y1)
                        {
                            chunk[x, y, z] = 2;
                        }
                        else if (wy < y2)
                        {
                            chunk[x, y, z] = 1;
                        }
                    }
                }
            }

            chunk.IsCreated = true;
        }
    }
}
