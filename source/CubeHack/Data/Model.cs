using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Data
{
    [EditorData]
    [ProtoContract]
    public class Model
    {
        [ProtoMember(1)]
        public int Index { get; set; }

        [EditorData]
        [ProtoMember(2)]
        public float Width { get; set; }

        [EditorData]
        [ProtoMember(3)]
        public float Height { get; set; }

        [EditorData]
        [ProtoMember(4)]
        public List<ModelCuboid> Cuboids { get; set; }
    }
}
