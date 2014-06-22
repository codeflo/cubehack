// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.GameData
{
    [ProtoContract]
    public class PhysicsValues
    {
        /// <summary>
        /// Gravity in blocks per square second.
        /// </summary>
        [ProtoMember(1)]
        public float Gravity { get; set; }

        /// <summary>
        /// Player movement speed in blocks per second.
        /// </summary>
        [ProtoMember(2)]
        public float PlayerMovementSpeed { get; set; }

        [ProtoMember(3)]
        public float PlayerWidth { get; set; }

        [ProtoMember(4)]
        public float PlayerHeight { get; set; }

        [ProtoMember(5)]
        public float PlayerEyeHeight { get; set; }

        [ProtoMember(6)]
        public float PlayerJumpHeight { get; set; }
    }
}
