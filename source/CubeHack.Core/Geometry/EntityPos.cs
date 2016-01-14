// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;
using ProtoBuf;

namespace CubeHack.Geometry
{
    [ProtoContract]
    public struct EntityPos
    {
        public static readonly EntityPos Origin = default(EntityPos);

        [ProtoMember(1)]
        public long X;

        [ProtoMember(2)]
        public long Y;

        [ProtoMember(3)]
        public long Z;

        private const double _scaleFactor = (double)(1L << 32);

        private const double _inverseScaleFactor = 1.0 / _scaleFactor;

        public EntityPos(long x, long y, long z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static explicit operator ChunkPos(EntityPos pos)
        {
            return new ChunkPos(
                (int)(pos.X >> (32 + GeometryConstants.ChunkSizeBits)),
                (int)(pos.Y >> (32 + GeometryConstants.ChunkSizeBits)),
                (int)(pos.Z >> (32 + GeometryConstants.ChunkSizeBits)));
        }

        public static implicit operator EntityPos(ChunkPos pos)
        {
            return new EntityPos(
                (long)pos.X << (32 + GeometryConstants.ChunkSizeBits),
                (long)pos.Y << (32 + GeometryConstants.ChunkSizeBits),
                (long)pos.Z << (32 + GeometryConstants.ChunkSizeBits));
        }

        public static explicit operator BlockPos(EntityPos pos)
        {
            return new BlockPos(
                (int)(pos.X >> 32),
                (int)(pos.Y >> 32),
                (int)(pos.Z >> 32));
        }

        public static implicit operator EntityPos(BlockPos pos)
        {
            return new EntityPos(
                (long)pos.X << 32,
                (long)pos.Y << 32,
                (long)pos.Z << 32);
        }

        public static bool operator ==(EntityPos a, EntityPos b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(EntityPos a, EntityPos b)
        {
            return !(a == b);
        }

        public static EntityOffset operator -(EntityPos a, EntityPos b)
        {
            return new EntityOffset((a.X - b.X) * _inverseScaleFactor, (a.Y - b.Y) * _inverseScaleFactor, (a.Z - b.Z) * _inverseScaleFactor);
        }

        public static EntityPos operator +(EntityPos a, EntityOffset b)
        {
            return new EntityPos(a.X + (long)(_scaleFactor * b.X), a.Y + (long)(_scaleFactor * b.Y), a.Z + (long)(_scaleFactor * b.Z));
        }

        public static EntityPos operator -(EntityPos p, EntityOffset o)
        {
            return p + (-o);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityPos && (EntityPos)obj == this;
        }

        public override int GetHashCode()
        {
            return HashCalculator.Value[X][Y][Z];
        }
    }
}
