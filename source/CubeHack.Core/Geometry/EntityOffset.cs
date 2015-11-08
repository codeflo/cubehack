// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;
using System;

namespace CubeHack.Geometry
{
    [ProtoContract]
    public struct EntityOffset
    {
        public const double Epsilon = 1.0 / (1L << 32);

        public EntityOffset(double x, double y, double z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        [ProtoMember(1)]
        public double X { get; set; }

        [ProtoMember(2)]
        public double Y { get; set; }

        [ProtoMember(3)]
        public double Z { get; set; }

        public double Length
        {
            get
            {
                return Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        public static EntityOffset operator +(EntityOffset o, EntityOffset p)
        {
            return new EntityOffset(o.X + p.X, o.Y + p.Y, o.Z + p.Z);
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
    }
}
