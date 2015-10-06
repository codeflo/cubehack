// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;

namespace CubeHack.Game
{
    public class World
    {
        private const int _chunkMask = Chunk.Size - 1;

        private const long _upperMask = unchecked((long)0xFFFFFFFF00000000L);

        private readonly Dictionary3D<Chunk> _chunkMap = new Dictionary3D<Chunk>();

        public WorldGenerator Generator { get; set; }

        public ushort this[int x, int y, int z]
        {
            get
            {
                if (y < -200)
                {
                    return 1;
                }

                var chunk = PeekChunk(x >> Chunk.Bits, y >> Chunk.Bits, z >> Chunk.Bits);
                if (chunk == null)
                {
                    return 0;
                }

                return chunk[x & _chunkMask, y & _chunkMask, z & _chunkMask];
            }

            set
            {
                var chunk = GetChunk(x >> Chunk.Bits, y >> Chunk.Bits, z >> Chunk.Bits);
                chunk[x & _chunkMask, y & _chunkMask, z & _chunkMask] = value;
            }
        }

        public Chunk GetChunk(int x, int y, int z)
        {
            var chunk = _chunkMap[x, y, z];

            if (chunk == null)
            {
                chunk = new Chunk(x, y, z);
                _chunkMap[x, y, z] = chunk;

                if (Generator != null)
                {
                    Generator.CreateChunk(chunk);
                }
            }

            return chunk;
        }

        public Chunk PeekChunk(int x, int y, int z)
        {
            return _chunkMap[x, y, z];
        }

        public void PasteChunkData(ChunkData chunkData)
        {
            GetChunk(chunkData.X, chunkData.Y, chunkData.Z).PasteChunkData(chunkData);
        }

        public RayCastResult CastRay(Position position, Offset direction, double max)
        {
            long x = position.X, y = position.Y, z = position.Z;
            double dx = direction.X, dy = direction.Y, dz = direction.Z;

            int sdx = (int)Math.Sign(dx);
            int sdy = (int)Math.Sign(dy);
            int sdz = (int)Math.Sign(dz);

            if (sdx == 0 && sdy == 0 && sdz == 0)
            {
                return null;
            }

            const long edgeLength = 1L << 32;
            const double oneOverEdge = 1.0 / edgeLength;

            int cx = (int)(x >> 32);
            int cy = (int)(y >> 32);
            int cz = (int)(z >> 32);

            if (this[cx, cy, cz] != 0)
            {
                return new RayCastResult { Position = position, CubeX = cx, CubeY = cy, CubeZ = cz };
            }

            while (max > 0 && IsNumber(max))
            {
                /* Check which edge of the current cube is hit first */

                long edgeX;
                if (sdx > 0)
                {
                    edgeX = (x + edgeLength) & _upperMask;
                }
                else
                {
                    edgeX = (x - 1) & _upperMask;
                }
                double tx = (edgeX - x) * oneOverEdge / dx;

                long edgeY;
                if (sdy > 0)
                {
                    edgeY = (y + edgeLength) & _upperMask;
                }
                else
                {
                    edgeY = (y - 1) & _upperMask;
                }
                double ty = (edgeY - y) * oneOverEdge / dy;

                long edgeZ;
                if (sdz > 0)
                {
                    edgeZ = (z + edgeLength) & _upperMask;
                }
                else
                {
                    edgeZ = (z - 1) & _upperMask;
                }
                double tz = (edgeZ - z) * oneOverEdge / dz;

                if (Math.Abs(edgeX - x) > edgeLength || Math.Abs(edgeY - y) > edgeLength
                        || Math.Abs(edgeZ - z) > edgeLength)
                {
                    // Shouldn't happen.
                    throw new Exception();
                }

                if (IsNumber(tx) && (tx <= ty || !IsNumber(ty)) && (tx <= tz || !IsNumber(tz)))
                {
                    x = edgeX;
                    y += (long)(tx * dy * edgeLength);
                    z += (long)(tx * dz * edgeLength);
                    max -= tx;

                    cx += sdx;
                    if (max >= 0 && this[cx, cy, cz] != 0)
                    {
                        return new RayCastResult { Position = new Position(x, y, z), CubeX = cx, CubeY = cy, CubeZ = cz, NormalX = -sdx, NormalY = 0, NormalZ = 0 };
                    }
                }
                else if (IsNumber(ty) && (ty <= tz || !IsNumber(tz)))
                {
                    x += (long)(ty * dx * edgeLength);
                    y = edgeY;
                    z += (long)(ty * dz * edgeLength);
                    max -= ty;

                    cy += sdy;
                    if (max >= 0 && this[cx, cy, cz] != 0)
                    {
                        return new RayCastResult { Position = new Position(x, y, z), CubeX = cx, CubeY = cy, CubeZ = cz, NormalX = 0, NormalY = -sdy, NormalZ = 0 };
                    }
                }
                else if (IsNumber(tz))
                {
                    x += (long)(tz * dx * edgeLength);
                    y += (long)(tz * dy * edgeLength);
                    z = edgeZ;
                    max -= tz;

                    cz += sdz;
                    if (max >= 0 && this[cx, cy, cz] != 0)
                    {
                        return new RayCastResult { Position = new Position(x, y, z), CubeX = cx, CubeY = cy, CubeZ = cz, NormalX = 0, NormalY = 0, NormalZ = -sdz };
                    }
                }
                else
                {
                    // Shouldn't happen.
                    throw new Exception();
                }
            }

            return null;
        }

        private static bool IsNumber(double d)
        {
            return !double.IsInfinity(d) && !double.IsNaN(d);
        }
    }
}
