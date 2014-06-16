// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.GameData;
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
        public List<PositionData> EntityPositions { get; set; }

        [ProtoMember(2)]
        public PhysicsValues PhysicsValues { get; set; }

        [ProtoMember(3)]
        public ChunkData ChunkData { get; set; }
    }
}
