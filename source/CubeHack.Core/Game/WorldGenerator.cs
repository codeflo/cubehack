// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

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
            for (int x = 0; x < Chunk.Size; ++x)
            {
                long wx = ((long)chunk.Pos.X << Chunk.Bits) + x;
                for (int z = 0; z < Chunk.Size; ++z)
                {
                    long wz = ((long)chunk.Pos.Z << Chunk.Bits) + z;

                    long y1 = (long)(8 * (Math.Sin(wx * 0.08 + 1) + Math.Sin(wz * 0.073 + 2))) - 16;
                    long y2 = (long)(3 * (Math.Sin(wx * 0.23 + wz * 0.02 + 1) + Math.Sin(-wx * 0.05 + wz * 0.31 + 2))) - 12;

                    for (int y = 0; y < Chunk.Size; ++y)
                    {
                        long wy = ((long)chunk.Pos.Y << Chunk.Bits) + y;

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
