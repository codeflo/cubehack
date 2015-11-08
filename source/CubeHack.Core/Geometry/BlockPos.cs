// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Geometry
{
    using Util;
    using ProtoBuf;

    /// <summary>
    /// Represents the coordinates of a single block in the game.
    /// </summary>
    [ProtoContract]
    public struct BlockPos
    {
        [ProtoMember(1)]
        public int X;

        [ProtoMember(2)]
        public int Y;

        [ProtoMember(3)]
        public int Z;

        public BlockPos(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(BlockPos a, BlockPos b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(BlockPos a, BlockPos b)
        {
            return !(a == b);
        }

        public static BlockOffset operator -(BlockPos a, BlockPos b)
        {
            return new BlockOffset(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static BlockPos operator +(BlockPos p, BlockOffset o)
        {
            return new BlockPos(p.X + o.X, p.Y + o.Y, p.Z + o.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockPos && (BlockPos)obj == this;
        }

        public override int GetHashCode()
        {
            return HashCalculator.Value[X][Y][Z];
        }
    }
}
