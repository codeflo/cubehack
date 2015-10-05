// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

namespace CubeHack.Util
{
    using System;

    public static class ExtraMath
    {
        public const double RadiansPerDegree = Math.PI / 180.0;
        public const double DegreesPerRadian = 180.0 / Math.PI;

        public static double Square(double p)
        {
            return p * p;
        }
    }
}
