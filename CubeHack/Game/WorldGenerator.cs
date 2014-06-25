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

        public WorldGenerator(World world)
        {
            _world = world;
        }

        public void CreateChunk(int chunkX, int chunkY, int chunkZ)
        {
            int x0 = chunkX << Chunk.Bits;
            int x1 = x0 + Chunk.Size;
            int y0 = -5;
            int y1 = 0;
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
            else if (chunkY == 0)
            {
                _world[x0 + 16, 0, z0 + 16] = 1;
            }
        }
    }
}
