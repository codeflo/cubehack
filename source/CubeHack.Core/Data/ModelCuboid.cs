// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

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
