// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    [ProtoContract]
    struct Offset
    {
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
