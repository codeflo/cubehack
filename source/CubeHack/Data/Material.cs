// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

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
    public class Material
    {
        [ProtoMember(1)]
        public int Index { get; set; }

        [EditorData]
        [ProtoMember(2)]
        public string Name { get; set; }

        [EditorData]
        [ProtoMember(3)]
        public Texture Texture { get; set; }
    }
}
