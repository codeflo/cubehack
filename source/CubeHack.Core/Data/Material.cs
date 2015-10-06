// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;

namespace CubeHack.Data
{
    [EditorData]
    [ProtoContract]
    public class Material
    {
        [ProtoMember(1)]
        public int Index { get; set; }

        [EditorData]
        [ProtoMember(2)]
        public string Name { get; set; }

        [EditorData]
        [ProtoMember(3)]
        public Texture Texture { get; set; }
    }
}
