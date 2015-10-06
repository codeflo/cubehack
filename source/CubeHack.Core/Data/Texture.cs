// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;

namespace CubeHack.Data
{
    [EditorData]
    [ProtoContract]
    public class Texture
    {
        public Texture()
        {
        }

        [EditorData]
        [ProtoMember(1)]
        public string Color { get; set; }

        public int Index { get; set; }
    }
}
