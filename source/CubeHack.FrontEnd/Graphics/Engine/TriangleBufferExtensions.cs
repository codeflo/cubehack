// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.FrontEnd.Graphics.Engine
{
    internal static class TriangleBufferExtensions
    {
        public static void VertexColorBytes(this TriangleBuffer buffer, float r, float g, float b, float a)
        {
            buffer.VertexByte(GetColorByte(r), GetColorByte(g), GetColorByte(b), GetColorByte(a));
        }

        private static byte GetColorByte(float v)
        {
            if (v <= 0) return 0;
            if (v >= 1) return 255;
            return (byte)(v * 255);
        }
    }
}
