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
    struct Position
    {
        private const double _scaleFactor = (double)(1L << 32);
        private const double _inverseScaleFactor = 1.0 / _scaleFactor;

        public Position(long x, long y, long z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        [ProtoMember(1)]
        public long X { get; set; }

        [ProtoMember(2)]
        public long Y { get; set; }

        [ProtoMember(3)]
        public long Z { get; set; }

        public int CubeX
        {
            get
            {
                return GetCubeCoordinate(X);
            }
        }

        public int CubeY
        {
            get
            {
                return GetCubeCoordinate(Y);
            }
        }

        public int CubeZ
        {
            get
            {
                return GetCubeCoordinate(Z);
            }
        }

        public static Offset operator -(Position a, Position b)
        {
            return new Offset((a.X - b.X) * _inverseScaleFactor, (a.Y - b.Y) * _inverseScaleFactor, (a.Z - b.Z) * _inverseScaleFactor);
        }

        public static Position operator +(Position a, Offset b)
        {
            return new Position(a.X + (long)(_scaleFactor * b.X), a.Y + (long)(_scaleFactor * b.Y), a.Z + (long)(_scaleFactor * b.Z));
        }

        public static Position operator -(Position a, Offset b)
        {
            return new Position(a.X - (long)(_scaleFactor * b.X), a.Y - (long)(_scaleFactor * b.Y), a.Z - (long)(_scaleFactor * b.Z));
        }

        public static int GetCubeCoordinate(long coordinate)
        {
            return (int)(coordinate >> 32);
        }
    }
}
