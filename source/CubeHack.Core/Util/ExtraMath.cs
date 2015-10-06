// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

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
