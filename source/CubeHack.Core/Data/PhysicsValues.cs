// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Data
{
    [EditorData]
    [ProtoContract]
    public class PhysicsValues
    {
        /// <summary>
        /// Gravity in blocks per square second.
        /// </summary>
        [EditorData]
        [ProtoMember(1)]
        public float Gravity { get; set; }

        /// <summary>
        /// Player movement speed in blocks per second.
        /// </summary>
        [EditorData]
        [ProtoMember(2)]
        public float PlayerMovementSpeed { get; set; }

        [EditorData]
        [ProtoMember(3)]
        public float PlayerWidth { get; set; }

        [EditorData]
        [ProtoMember(4)]
        public float PlayerHeight { get; set; }

        [EditorData]
        [ProtoMember(5)]
        public float PlayerEyeHeight { get; set; }

        [EditorData]
        [ProtoMember(6)]
        public float PlayerJumpHeight { get; set; }

        [EditorData]
        [ProtoMember(7)]
        public float MiningDistance { get; set; }

        [EditorData]
        [ProtoMember(8)]
        public float MiningTime { get; set; }

        [EditorData]
        [ProtoMember(9)]
        public float PlacementCooldown { get; set; }

        [EditorData]
        [ProtoMember(10)]
        public float TerminalHeight { get; set; }
    }
}
