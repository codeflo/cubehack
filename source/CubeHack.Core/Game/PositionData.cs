// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;

namespace CubeHack.Game
{
    [ProtoContract]
    public class PositionData
    {
        [ProtoMember(1)]
        public Position Position;

        [ProtoMember(2)]
        public Offset Velocity;

        [ProtoMember(3)]
        public float HAngle;

        [ProtoMember(4)]
        public float VAngle;

        [ProtoMember(5)]
        public bool IsFalling;

        [ProtoMember(6)]
        public Position CollisionPosition;
    }
}
