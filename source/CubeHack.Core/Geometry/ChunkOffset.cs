// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Geometry
{
    using ProtoBuf;
    using Util;

    /// <summary>
    /// The offset between two <see cref="Geometry.ChunkPos"/> values.
    /// </summary>
    public struct ChunkOffset
    {
        [ProtoMember(1)]
        public int X;

        [ProtoMember(2)]
        public int Y;

        [ProtoMember(3)]
        public int Z;

        public ChunkOffset(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static ChunkOffset operator +(ChunkOffset a, ChunkOffset b)
        {
            return new ChunkOffset(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static ChunkOffset operator -(ChunkOffset a, ChunkOffset b)
        {
            return new ChunkOffset(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static ChunkOffset operator -(ChunkOffset o)
        {
            return new ChunkOffset(-o.X, -o.Y, -o.Z);
        }

        public static bool operator ==(ChunkOffset a, ChunkOffset b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(ChunkOffset a, ChunkOffset b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkOffset && (ChunkOffset)obj == this;
        }

        public override int GetHashCode()
        {
            return HashCalculator.Value[X][Y][Z];
        }
    }
}
