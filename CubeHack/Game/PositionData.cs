﻿// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    [ProtoContract]
    class PositionData
    {
        [ProtoMember(1)]
        public Position Position { get; set; }

        [ProtoMember(2)]
        public Offset Velocity { get; set; }

        [ProtoMember(3)]
        public float HAngle { get; set; }

        [ProtoMember(4)]
        public float VAngle { get; set; }
    }
}
