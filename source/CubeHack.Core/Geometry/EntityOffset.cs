// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;
using ProtoBuf;
using System;

namespace CubeHack.Geometry
{
    [ProtoContract]
    public struct EntityOffset
    {
        public const double Epsilon = 1.0 / (1L << 32);

        public static readonly EntityOffset Zero = default(EntityOffset);
        public static readonly EntityOffset UnitX = new EntityOffset(1, 0, 0);
        public static readonly EntityOffset UnitY = new EntityOffset(0, 1, 0);
        public static readonly EntityOffset UnitZ = new EntityOffset(0, 0, 1);

        [ProtoMember(1)]
        public double X;

        [ProtoMember(2)]
        public double Y;

        [ProtoMember(3)]
        public double Z;

        public EntityOffset(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

        public static EntityOffset operator +(EntityOffset a, EntityOffset b)
        {
            return new EntityOffset(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static EntityOffset operator -(EntityOffset a, EntityOffset b)
        {
            return new EntityOffset(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static EntityOffset operator -(EntityOffset o)
        {
            return new EntityOffset(-o.X, -o.Y, -o.Z);
        }

        public static EntityOffset operator *(double f, EntityOffset o)
        {
            return new EntityOffset(f * o.X, f * o.Y, f * o.Z);
        }

        public static EntityOffset operator *(EntityOffset o, double f)
        {
            return new EntityOffset(f * o.X, f * o.Y, f * o.Z);
        }

        public static EntityOffset operator /(EntityOffset o, double d)
        {
            double f = 1.0 / d;
            return new EntityOffset(f * o.X, f * o.Y, f * o.Z);
        }

        public static bool operator ==(EntityOffset a, EntityOffset b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(EntityOffset a, EntityOffset b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is EntityOffset && (EntityOffset)obj == this;
        }

        public override int GetHashCode()
        {
            return HashCalculator.Value[X][Y][Z];
        }
    }
}
