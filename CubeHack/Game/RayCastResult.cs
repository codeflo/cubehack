// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    class RayCastResult
    {
        public Position Position { get; set; }

        public int CubeX { get; set; }
        public int CubeY { get; set; }
        public int CubeZ { get; set; }

        public int NormalX { get; set; }
        public int NormalY { get; set; }
        public int NormalZ { get; set; }
    }
}
