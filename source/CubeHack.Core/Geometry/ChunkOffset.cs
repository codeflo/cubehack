// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Geometry
{
    using ProtoBuf;

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
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static ChunkOffset operator -(ChunkOffset o)
        {
            return new ChunkOffset(-o.X, -o.Y, -o.Z);
        }
    }
}
