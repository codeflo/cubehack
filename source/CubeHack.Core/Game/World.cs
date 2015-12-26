// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Geometry;
using CubeHack.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    public class World
    {
        private const int _chunkMask = GeometryConstants.ChunkSize - 1;

        private const long _upperMask = unchecked((long)0xFFFFFFFF00000000L);

        private readonly object _mutex = new object();

        private readonly Dictionary<ChunkPos, Chunk> _chunkMap = new Dictionary<ChunkPos, Chunk>();

        public World(Universe universe)
        {
            Universe = universe;
        }

        public Universe Universe { get; }

        public WorldGenerator Generator { get; set; }

        public ushort this[BlockPos p]
        {
            get
            {
                if (p.Y < -200)
                {
                    return 1;
                }

                var chunk = PeekChunk((ChunkPos)p);
                if (chunk == null)
                {
                    return 0;
                }

                return chunk[p.X & _chunkMask, p.Y & _chunkMask, p.Z & _chunkMask];
            }

            set
            {
                var chunkPos = (ChunkPos)p;
                var chunk = GetChunk(chunkPos);
                if (chunk.IsCreated)
                {
                    chunk[p.X & _chunkMask, p.Y & _chunkMask, p.Z & _chunkMask] = value;
                    Universe?.SaveFile?.Write(StorageKey.Get("ChunkData", chunkPos), StorageValue.Serialize(chunk.GetChunkData()));
                }
            }
        }

        public Chunk GetChunk(ChunkPos chunkPos)
        {
            lock (_mutex)
            {
                Chunk chunk;
                if (!_chunkMap.TryGetValue(chunkPos, out chunk))
                {
                    chunk = new Chunk(this, chunkPos);
                    _chunkMap[chunkPos] = chunk;
                    LoadOrGenerate(chunk);
                }

                return chunk;
            }
        }

        public Chunk PeekChunk(ChunkPos chunkPos)
        {
            Chunk chunk;
            _chunkMap.TryGetValue(chunkPos, out chunk);
            return chunk;
        }

        public void PasteChunkData(ChunkData chunkData)
        {
            GetChunk(chunkData.Pos).PasteChunkData(chunkData);
        }

        public RayCastResult CastRay(EntityPos entityPos, EntityOffset direction, double max)
        {
            long x = entityPos.X, y = entityPos.Y, z = entityPos.Z;
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

            var blockPos = (BlockPos)entityPos;

            if (this[blockPos] != 0)
            {
                return new RayCastResult { EntityPos = entityPos, BlockPos = blockPos };
            }

            while (max > 0 && IsNumber(max))
            {
                /* Check which edge of the current block is hit first */

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

                    blockPos.X += sdx;
                    if (max >= 0 && this[blockPos] != 0)
                    {
                        return new RayCastResult { EntityPos = new EntityPos(x, y, z), BlockPos = blockPos, Normal = new BlockOffset(-sdx, 0, 0) };
                    }
                }
                else if (IsNumber(ty) && (ty <= tz || !IsNumber(tz)))
                {
                    x += (long)(ty * dx * edgeLength);
                    y = edgeY;
                    z += (long)(ty * dz * edgeLength);
                    max -= ty;

                    blockPos.Y += sdy;
                    if (max >= 0 && this[blockPos] != 0)
                    {
                        return new RayCastResult { EntityPos = new EntityPos(x, y, z), BlockPos = blockPos, Normal = new BlockOffset(0, -sdy, 0) };
                    }
                }
                else if (IsNumber(tz))
                {
                    x += (long)(tz * dx * edgeLength);
                    y += (long)(tz * dy * edgeLength);
                    z = edgeZ;
                    max -= tz;

                    blockPos.Z += sdz;
                    if (max >= 0 && this[blockPos] != 0)
                    {
                        return new RayCastResult { EntityPos = new EntityPos(x, y, z), BlockPos = blockPos, Normal = new BlockOffset(0, 0, -sdz) };
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

        private async void LoadOrGenerate(Chunk chunk)
        {
            var savedValueTask = Universe?.SaveFile?.ReadAsync(StorageKey.Get("ChunkData", chunk.Pos));
            StorageValue savedValue = savedValueTask == null ? null : await savedValueTask;
            if (savedValue != null)
            {
                var chunkData = savedValue.Deserialize<ChunkData>();
                if (chunkData != null)
                {
                    chunk.PasteChunkData(savedValue.Deserialize<ChunkData>());
                }
            }
            else if (Generator != null)
            {
                await Task.Run(() => Generator.CreateChunk(chunk));
            }
        }
    }
}
