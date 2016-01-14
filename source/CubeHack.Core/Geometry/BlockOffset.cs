// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Geometry
{
    using ProtoBuf;
    using Util;

    /// <summary>
    /// The offset between two <see cref="Geometry.BlockPos"/> values.
    /// </summary>
    public struct BlockOffset
    {
        public static readonly BlockOffset Zero = default(BlockOffset);
        public static readonly BlockOffset UnitX = new BlockOffset(1, 0, 0);
        public static readonly BlockOffset UnitY = new BlockOffset(0, 1, 0);
        public static readonly BlockOffset UnitZ = new BlockOffset(0, 0, 1);

        [ProtoMember(1)]
        public int X;

        [ProtoMember(2)]
        public int Y;

        [ProtoMember(3)]
        public int Z;

        public BlockOffset(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static BlockOffset operator +(BlockOffset a, BlockOffset b)
        {
            return new BlockOffset(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static BlockOffset operator -(BlockOffset a, BlockOffset b)
        {
            return new BlockOffset(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static BlockOffset operator -(BlockOffset o)
        {
            return new BlockOffset(-o.X, -o.Y, -o.Z);
        }

        public static bool operator ==(BlockOffset a, BlockOffset b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(BlockOffset a, BlockOffset b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockOffset && (BlockOffset)obj == this;
        }

        public override int GetHashCode()
        {
            return HashCalculator.Value[X][Y][Z];
        }
    }
}
