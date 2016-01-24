// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Geometry;
using CubeHack.State;
using ProtoBuf;

namespace CubeHack.Game
{
    [Component("Position")]
    [ProtoContract]
    public class PositionComponent
    {
        [ProtoMember(1)]
        public EntityPlacement Placement;

        [ProtoMember(2)]
        public EntityOffset Velocity;

        [ProtoMember(3)]
        public bool IsFalling;

        [ProtoMember(4)]
        public EntityPos InternalPos;
    }
}
