using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    [ProtoContract]
    class ChunkData
    {
        [ProtoMember(1)]
        public int X0 { get; set; }

        [ProtoMember(2)]
        public int X1 { get; set; }

        [ProtoMember(3)]
        public int Y0 { get; set; }

        [ProtoMember(4)]
        public int Y1 { get; set; }

        [ProtoMember(5)]
        public int Z0 { get; set; }

        [ProtoMember(6)]
        public int Z1 { get; set; }

        [ProtoMember(7)]
        private ushort[] InternalData { get; set; }

        public ushort this[int x, int y, int z]
        {
            get
            {
                if (InternalData == null)
                {
                    return 0;
                }

                return InternalData[GetIndex(x, y, z)];
            }

            set
            {
                if (InternalData == null)
                {
                    InternalData = new ushort[(X1 - X0) * (Y1 - Y0) * (Z1 - Z0)];
                }

                InternalData[GetIndex(x, y, z)] = value;
            }
        }

        private int GetIndex(int x, int y, int z)
        {
            return ((x - X0) * (Y1 - Y0) + (y - Y0)) * (Z1 - Z0) + (z - Z0);
        }
    }
}
