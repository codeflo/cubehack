// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

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
