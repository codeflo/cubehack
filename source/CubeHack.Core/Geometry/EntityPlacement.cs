// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;

namespace CubeHack.Geometry
{
    [ProtoContract]
    public struct EntityPlacement
    {
        [ProtoMember(1)]
        public EntityPos Pos;

        [ProtoMember(2)]
        public EntityOrientation Orientation;

        public EntityPlacement(EntityPos pos, EntityOrientation orientation)
        {
            Pos = pos;
            Orientation = orientation;
        }
    }
}
