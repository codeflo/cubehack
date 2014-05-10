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
    class GameEvent
    {
        [ProtoMember(1)]
        public List<EntityData> Entities { get; set; }

        [ProtoContract]
        public class EntityData
        {
            [ProtoMember(1)]
            public float X { get; set; }

            [ProtoMember(2)]
            public float Y { get; set; }

            [ProtoMember(3)]
            public float Z { get; set; }

            [ProtoMember(4)]
            public float VX { get; set; }

            [ProtoMember(5)]
            public float VY { get; set; }

            [ProtoMember(6)]
            public float VZ { get; set; }
        }
    }
}
