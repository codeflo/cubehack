// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using ProtoBuf;
using System.Collections.Generic;

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
