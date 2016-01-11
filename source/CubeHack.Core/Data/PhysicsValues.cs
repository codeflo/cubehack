// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;

namespace CubeHack.Data
{
    [EditorData]
    [ProtoContract]
    public class PhysicsValues
    {
        /// <summary>
        /// Gets or sets the gravity in blocks per square second.
        /// </summary>
        [EditorData]
        [ProtoMember(1)]
        public float Gravity { get; set; }

        /// <summary>
        /// Gets or sets the player movement speed in blocks per second.
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
