// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Game
{
    public class RayCastResult
    {
        public Position Position { get; set; }

        public int CubeX { get; set; }

        public int CubeY { get; set; }

        public int CubeZ { get; set; }

        public int NormalX { get; set; }

        public int NormalY { get; set; }

        public int NormalZ { get; set; }

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

            return a.CubeX == b.CubeX
                && a.CubeY == b.CubeY
                && a.CubeZ == b.CubeZ
                && a.NormalX == b.NormalX
                && a.NormalY == b.NormalY
                && a.NormalZ == b.NormalZ;
        }
    }
}
