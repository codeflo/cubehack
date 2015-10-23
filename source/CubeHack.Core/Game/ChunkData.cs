// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Geometry;
using ProtoBuf;

namespace CubeHack.Game
{
    [ProtoContract]
    public class ChunkData
    {
        [ProtoMember(1)]
        public ChunkPos Pos { get; set; }

        [ProtoMember(2)]
        public bool IsCreated { get; set; }

        [ProtoMember(3)]
        public byte[] Data { get; set; }
    }
}
