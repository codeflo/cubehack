// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Data;
using ProtoBuf;
using System.Collections.Generic;

namespace CubeHack.Game
{
    [ProtoContract]
    public class ModData
    {
        [ProtoMember(1)]
        public List<Material> Materials { get; set; }

        [ProtoMember(2)]
        public List<Model> Models { get; set; }
    }
}
