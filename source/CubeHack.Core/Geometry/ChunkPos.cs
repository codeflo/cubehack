// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Geometry
{
    using Game;
    using ProtoBuf;
    using Util;

    /// <summary>
    /// Represents the coordinates of a <see cref="Game.Chunk"/> (a group of blocks with a fixed size).
    /// </summary>
    [ProtoContract]
    public struct ChunkPos
    {
        [ProtoMember(1)]
        public int X; 

        [ProtoMember(2)]
        public int Y;

        [ProtoMember(3)]
        public int Z;
        
        public ChunkPos(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(ChunkPos a, ChunkPos b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(ChunkPos a, ChunkPos b)
        {
            return !(a == b);
        }

        public static ChunkOffset operator -(ChunkPos a, ChunkPos b)
        {
            return new ChunkOffset(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static ChunkPos operator +(ChunkPos p, ChunkOffset o)
        {
            return new ChunkPos(p.X + o.X, p.Y + o.Y, p.Z + o.Z);
        }

        public static ChunkPos operator -(ChunkPos p, ChunkOffset o)
        {
            return p + (-o);
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkPos && (ChunkPos)obj == this;
        }

        public override int GetHashCode()
        {
            return HashCalculator.Value[X][Y][Z];
        }
    }
}
