// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;

namespace CubeHack.Data
{
    [EditorData]
    [ProtoContract]
    public class MobType
    {
        [EditorData]
        [ProtoMember(1)]
        public string Name { get; set; }

        [EditorData]
        [ProtoMember(2)]
        public Model Model { get; set; }
    }
}
