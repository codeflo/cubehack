// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;

namespace CubeHack.Util
{
    public static class Rng
    {
        /* TODO: Replace this with a more robust RNG, something like Mersenne Twister perhaps? */

        private static Random _random = new Random();

        /// <summary>
        /// Generates a random number with an exponential distribution and expected value 1.
        /// </summary>
        /// <returns>A random number with an exponential distribution.</returns>
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
