// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Geometry
{
    using ProtoBuf;
    using System;
    using Util;

    /// <summary>
    /// Represents the coordinates of a <see cref="Game.Chunk"/> (a group of blocks with a fixed size).
    /// </summary>
    [ProtoContract]
    public struct ChunkPos
    {
        [ProtoMember(1)]
        public int X;

        [ProtoMember(2)]
        public int Y;

        [ProtoMember(3)]
        public int Z;

        public ChunkPos(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(ChunkPos a, ChunkPos b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(ChunkPos a, ChunkPos b)
        {
            return !(a == b);
        }

        public static ChunkOffset operator -(ChunkPos a, ChunkPos b)
        {
            return new ChunkOffset(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static ChunkPos operator +(ChunkPos p, ChunkOffset o)
        {
            return new ChunkPos(p.X + o.X, p.Y + o.Y, p.Z + o.Z);
        }

        public static ChunkPos operator -(ChunkPos p, ChunkOffset o)
        {
            return p + (-o);
        }

        /// <summary>
        /// Iterates over the a rectangular box of ChunkPos values around a given center, starting with the center and working outwards.
        /// </summary>
        /// <param name="center">The center position of the chunks to iterate.</param>
        /// <param name="radiusXZ">The "radius" of the box in X and Z coordinates.</param>
        /// <param name="radiusY">The "radius" of the box in Y coordinates.</param>
        /// <param name="action">The action to call for each ChunkPos.</param>
        public static void IterateOutwards(ChunkPos center, int radiusXZ, int radiusY, Action<ChunkPos> action)
        {
            var radius = Math.Max(radiusXZ, radiusY);
            for (var r = 0; r <= radius; ++r)
            {
                // Only send the ChunkPos values with exact distance r from the center.

                var ry = Math.Min(r, radiusY);
                for (var y = -ry; y <= ry; ++y)
                {
                    if (y == -r || y == r)
                    {
                        var rxz = Math.Min(r, radiusXZ);

                        for (var x = -rxz; x <= rxz; ++x)
                        {
                            for (var z = -rxz; z <= rxz; ++z)
                            {
                                action(new ChunkPos(center.X + x, center.Y + y, center.Z + z));
                            }
                        }
                    }
                    else if (r <= radiusXZ)
                    {
                        for (var i = 0; i <= 2 * r; ++i)
                        {
                            action(new ChunkPos(center.X - r + i, center.Y + y, center.Z - r));
                            action(new ChunkPos(center.X + r, center.Y + y, center.Z - r + i));
                            action(new ChunkPos(center.X + r - i, center.Y + y, center.Z + r));
                            action(new ChunkPos(center.X - r, center.Y + y, center.Z + r - i));
                        }
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkPos && (ChunkPos)obj == this;
        }

        public override int GetHashCode()
        {
            return HashCalculator.Value[X][Y][Z];
        }
    }
}
