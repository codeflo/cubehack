// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    class WorldGenerator
    {
        private readonly World _world;
        private readonly Random _random = new Random();

        public WorldGenerator(World world)
        {
            _world = world;
        }

        public void CreateChunk(int chunkX, int chunkY, int chunkZ)
        {
            int x0 = chunkX << Chunk.Bits;
            int x1 = x0 + Chunk.Size;
            int y0 = (chunkY << Chunk.Bits) + 16;
            int y1 = y0 + 1 + _random.Next(8);
            int z0 = chunkZ << Chunk.Bits;
            int z1 = z0 + Chunk.Size;

            if (chunkY == -1)
            {
                for (int x = x0; x < x1; ++x)
                {
                    for (int y = y0; y < y1; ++y)
                    {
                        for (int z = z0; z < z1; ++z)
                        {
                            _world[x, y, z] = 1;
                        }
                    }
                }
            }
        }
    }
}
