﻿// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Game;
using ProtoBuf;

namespace CubeHack.Geometry
{
    [ProtoContract]
    public struct EntityPos
    {
        private const double _scaleFactor = (double)(1L << 32);
        private const double _inverseScaleFactor = 1.0 / _scaleFactor;

        public EntityPos(long x, long y, long z)
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

        public int ChunkX
        {
            get
            {
                return GetChunkCoordinate(X);
            }
        }

        public int ChunkY
        {
            get
            {
                return GetChunkCoordinate(Y);
            }
        }

        public int ChunkZ
        {
            get
            {
                return GetChunkCoordinate(Z);
            }
        }

        public static EntityOffset operator -(EntityPos a, EntityPos b)
        {
            return new EntityOffset((a.X - b.X) * _inverseScaleFactor, (a.Y - b.Y) * _inverseScaleFactor, (a.Z - b.Z) * _inverseScaleFactor);
        }

        public static EntityPos operator +(EntityPos a, EntityOffset b)
        {
            return new EntityPos(a.X + (long)(_scaleFactor * b.X), a.Y + (long)(_scaleFactor * b.Y), a.Z + (long)(_scaleFactor * b.Z));
        }

        public static EntityPos operator -(EntityPos a, EntityOffset b)
        {
            return new EntityPos(a.X - (long)(_scaleFactor * b.X), a.Y - (long)(_scaleFactor * b.Y), a.Z - (long)(_scaleFactor * b.Z));
        }

        public static int GetCubeCoordinate(long coordinate)
        {
            return (int)(coordinate >> 32);
        }

        public static int GetChunkCoordinate(long coordinate)
        {
            return (int)(coordinate >> (32 + Chunk.Bits));
        }
    }
}