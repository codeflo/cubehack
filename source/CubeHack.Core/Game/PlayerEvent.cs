// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;
using System.Collections.Generic;

namespace CubeHack.Game
{
    [ProtoContract]
    public class PlayerEvent
    {
        [ProtoMember(1)]
        public PositionComponent PositionData { get; set; }

        [ProtoMember(2)]
        public List<BlockUpdateData> BlockUpdates { get; set; }
    }
}
