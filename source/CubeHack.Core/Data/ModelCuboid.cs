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
    public class ModelCuboid
    {
        [EditorData]
        [ProtoMember(1)]
        public float CenterX { get; set; }

        [EditorData]
        [ProtoMember(2)]
        public float CenterY { get; set; }

        [EditorData]
        [ProtoMember(3)]
        public float CenterZ { get; set; }

        [EditorData]
        [ProtoMember(4)]
        public float RadiusX { get; set; }

        [EditorData]
        [ProtoMember(5)]
        public float RadiusY { get; set; }

        [EditorData]
        [ProtoMember(6)]
        public float RadiusZ { get; set; }

        [EditorData]
        [ProtoMember(7)]
        public Texture Texture { get; set; }
    }
}
