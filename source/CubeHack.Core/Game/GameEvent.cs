// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.State;
using ProtoBuf;
using System.Collections.Generic;

namespace CubeHack.Game
{
    [ProtoContract]
    public sealed class GameEvent
    {
        [ProtoMember(1)]
        public List<EntityData> EntityInfos { get; set; }

        [ProtoMember(2)]
        public PhysicsValues PhysicsValues { get; set; }

        [ProtoMember(3)]
        public List<ChunkData> ChunkDataList { get; set; }

        [ProtoMember(4)]
        public List<BlockUpdateData> BlockUpdates { get; set; }

        [ProtoMember(5)]
        public bool IsDisconnected { get; set; }
    }
}
