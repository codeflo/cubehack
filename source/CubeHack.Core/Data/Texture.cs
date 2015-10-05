// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

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
