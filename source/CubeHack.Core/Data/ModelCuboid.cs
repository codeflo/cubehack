// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;

namespace CubeHack.Data
{
    [EditorData]
    [ProtoContract]
    public class ModelCuboid
    {
        [EditorData]
        [ProtoMember(1)]
        public float CenterX { get; set; }

        [EditorData]
        [ProtoMember(2)]
        public float CenterY { get; set; }

        [EditorData]
        [ProtoMember(3)]
        public float CenterZ { get; set; }

        [EditorData]
        [ProtoMember(4)]
        public float RadiusX { get; set; }

        [EditorData]
        [ProtoMember(5)]
        public float RadiusY { get; set; }

        [EditorData]
        [ProtoMember(6)]
        public float RadiusZ { get; set; }

        [EditorData]
        [ProtoMember(7)]
        public Texture Texture { get; set; }
    }
}
