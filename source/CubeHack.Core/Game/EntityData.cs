﻿// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;

namespace CubeHack.Game
{
    [ProtoContract]
    public class EntityData
    {
        [ProtoMember(1)]
        public PositionComponent PositionData;

        [ProtoMember(2)]
        public int? ModelIndex;
    }
}
