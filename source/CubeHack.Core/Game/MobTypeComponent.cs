// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.State;
using ProtoBuf;

namespace CubeHack.Game
{
    [Component("MobType")]
    [ProtoContract]
    public sealed class MobTypeComponent
    {
        public MobTypeComponent()
        {
        }

        public MobTypeComponent(string mobType)
        {
            MobType = mobType;
        }

        [ProtoMember(1)]
        public string MobType { get; set; }
    }
}
