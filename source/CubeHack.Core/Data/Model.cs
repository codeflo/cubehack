// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

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
