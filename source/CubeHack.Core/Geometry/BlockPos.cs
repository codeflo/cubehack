using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using CubeHack.Util;
using ProtoBuf;

namespace CubeHack.Geometry
{
    [ProtoContract]
    public struct BlockPos
    {
        [ProtoMember(1)]
        public int X;

        [ProtoMember(2)]
        public int Y;

        [ProtoMember(3)]
        public int Z;

        public BlockPos(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(BlockPos a, BlockPos b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(BlockPos a, BlockPos b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockPos && (BlockPos)obj == this;
        }

        public override int GetHashCode()
        {
            return HashCalculator.Value[X][Y][Z];
        }
    }
}
