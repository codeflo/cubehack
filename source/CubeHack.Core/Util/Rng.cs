// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Util
{
    public static class Rng
    {
        // TODO: Replace this with a more robust RNG, something like Mersenne Twister perhaps?

        static Random _random = new Random();

        public static double NextExp()
        {
            return -Math.Log(_random.NextDouble());
        }

        public static double NextDouble()
        {
            return _random.NextDouble();
        }

        public static float NextFloat()
        {
            return (float)NextDouble();
        }
    }
}
