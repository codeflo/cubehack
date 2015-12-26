// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Geometry;
using ProtoBuf;

namespace CubeHack.Game
{
    [ProtoContract]
    public class BlockUpdateData
    {
        [ProtoMember(1)]
        public BlockPos Pos;

        [ProtoMember(2)]
        public ushort Material { get; set; }
    }
}
