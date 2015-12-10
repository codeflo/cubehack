// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Geometry
{
    using ProtoBuf;

    /// <summary>
    /// The offset between two <see cref="Geometry.BlockPos"/> values.
    /// </summary>
    public struct BlockOffset
    {
        [ProtoMember(1)]
        public int X;

        [ProtoMember(2)]
        public int Y;

        [ProtoMember(3)]
        public int Z;

        public BlockOffset(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static BlockOffset operator -(BlockOffset o)
        {
            return new BlockOffset(-o.X, -o.Y, -o.Z);
        }
    }
}
