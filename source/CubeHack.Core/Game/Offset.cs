// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;
using System;

namespace CubeHack.Game
{
    [ProtoContract]
    public struct Offset
    {
        public const double Epsilon = 1.0 / (1L << 32);

        public Offset(double x, double y, double z)
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

        public static Offset operator +(Offset o, Offset p)
        {
            return new Offset(o.X + p.X, o.Y + p.Y, o.Z + p.Z);
        }

        public static Offset operator *(double f, Offset o)
        {
            return new Offset(f * o.X, f * o.Y, f * o.Z);
        }

        public static Offset operator *(Offset o, double f)
        {
            return new Offset(f * o.X, f * o.Y, f * o.Z);
        }

        public static Offset operator /(Offset o, double d)
        {
            double f = 1.0 / d;
            return new Offset(f * o.X, f * o.Y, f * o.Z);
        }
    }
}
