// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Geometry
{
    public static class GeometryConstants
    {
        public const int ChunkSizeBits = 5;
        public const int ChunkSize = 1 << ChunkSizeBits;

        public const int ChunkViewRadiusXZ = 5;
        public const int ChunkViewRadiusY = 3;
    }
}
