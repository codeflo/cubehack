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
    public class MobType
    {
        [EditorData]
        [ProtoMember(1)]
        public string Name { get; set; }

        [EditorData]
        [ProtoMember(2)]
        public Model Model { get; set; }
    }
}
