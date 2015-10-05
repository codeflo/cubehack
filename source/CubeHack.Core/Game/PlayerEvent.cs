// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using ProtoBuf;
using System.Collections.Generic;

namespace CubeHack.Game
{
    [ProtoContract]
    public class PlayerEvent
    {
        [ProtoMember(1)]
        public PositionData PositionData { get; set; }

        [ProtoMember(2)]
        public List<CubeUpdateData> CubeUpdates { get; set; }
    }
}
