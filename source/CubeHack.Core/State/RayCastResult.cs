// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Geometry;

namespace CubeHack.State
{
    public sealed class RayCastResult
    {
        public BlockPos BlockPos;

        public BlockOffset Normal;

        public EntityPos EntityPos;

        public static bool FaceEquals(RayCastResult a, RayCastResult b)
        {
            if (a == null)
            {
                return b == null;
            }

            if (b == null)
            {
                return false;
            }

            return a.BlockPos == b.BlockPos
                && a.Normal == b.Normal;
        }
    }
}
