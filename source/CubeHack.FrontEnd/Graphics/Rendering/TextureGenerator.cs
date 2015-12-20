// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.Util;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace CubeHack.FrontEnd.Graphics.Rendering
{
    internal static class TextureGenerator
    {
        public const int TextureSize = WorldTextureAtlas.TextureSize;

        private static readonly NoiseCube _noiseCube = new NoiseCube();

        public static void DrawTexture(BitmapData bitmapData, Texture texture, int offsetX, int offsetY)
        {
            if (texture == null) return;

            var color = new CubeHack.Data.Color(texture.Color);

            double tf = 1.0 / TextureSize;
            for (int x = 0; x < TextureSize; ++x)
            {
                double xf = x * tf;
                for (int y = 0; y < TextureSize; ++y)
                {
                    double yf = y * tf;

                    float r = color.R;
                    float g = color.G;
                    float b = color.B;

                    float o = 10 * (_noiseCube.Get(xf, yf, 0.075) - _noiseCube.Get(xf, yf, 0.10));
                    if (texture.Index == 2)
                    {
                        o = o > 0.070f ? 0.070f : 0;
                        r += o;
                        g += o;
                        b += o;
                    }
                    else
                    {
                        o = Math.Abs(o) < 0.05 ? 0.80f : 1;
                        r *= o;
                        g *= o;
                        b *= o;
                    }

                    SetPixel(bitmapData, offsetX, offsetY, x, y, r, g, b);
                }
            }
        }

        private static void SetPixel(BitmapData bitmapData, int offsetX, int offsetY, int x, int y, float r, float g, float b)
        {
            var ptr = bitmapData.Scan0 + (y + offsetY) * bitmapData.Stride + (x + offsetX) * 4;
            uint v = GetColorValue(NormalizeColor(r), NormalizeColor(g), NormalizeColor(b), 1.0f);

            unsafe
            {
                *(uint*)ptr = v;
            }
        }

        private static float NormalizeColor(float r)
        {
            return r > 1 ? 1 : (r < 0 ? 0 : r);
        }

        private static uint GetColorValue(float r, float g, float b, float a)
        {
            return GetByte(b) | (GetByte(g) << 8) | (GetByte(r) << 16) | (GetByte(a) << 24);
        }

        private static uint GetByte(float v)
        {
            if (v < 0) v = 0;
            uint i = (uint)(v * 255);
            if (i > 255) i = 255;
            return i;
        }

        private static Brush BrushFromColor(CubeHack.Data.Color c)
        {
            return new SolidBrush(System.Drawing.Color.FromArgb(
                IntFromFloatColor(c.R),
                IntFromFloatColor(c.G),
                IntFromFloatColor(c.B)));
        }

        private static int IntFromFloatColor(float v)
        {
            return (int)(v * 255f);
        }

        private class NoiseCube
        {
            private const int noiseSize = 64;
            private const double noiseFactor = 1.0 / noiseSize;
            private double[,] _data = new double[noiseSize, noiseSize];

            public NoiseCube()
            {
                double avg = 0;

                for (int x = 0; x < noiseSize; ++x)
                {
                    for (int y = 0; y < noiseSize; ++y)
                    {
                        _data[x, y] = 2 * Rng.NextDouble() - 1;
                        avg += _data[x, y];
                    }
                }

                avg /= noiseSize * noiseSize;
                for (int x = 0; x < noiseSize; ++x)
                {
                    for (int y = 0; y < noiseSize; ++y)
                    {
                        _data[x, y] -= avg;
                    }
                }
            }

            public float Get(double x, double y, double r)
            {
                double mr2 = -1.0 / (r * r);

                int d = Math.Min((int)Math.Ceiling(2 * r * noiseSize), noiseSize - 1);
                int x0 = (int)Math.Round(x * noiseSize);
                int y0 = (int)Math.Round(y * noiseSize);

                double v = 0, vn = 0;
                for (int x1 = x0 - d; x1 <= x0 + d; ++x1)
                {
                    int x2 = x1 & (noiseSize - 1);
                    double xd = x1 * noiseFactor - x;
                    for (int y1 = y0 - d; y1 <= y0 + d; ++y1)
                    {
                        int y2 = y1 & (noiseSize - 1);
                        double yd = y1 * noiseFactor - y;
                        double e = Math.Exp(mr2 * (xd * xd + yd * yd));
                        v += e * _data[x2, y2];
                        vn += e;
                    }
                }

                return (float)(v / vn);
            }

            private double Normalize(double p)
            {
                if (p > 0.5)
                {
                    do
                    {
                        p -= 1.0;
                    }
                    while (p > 0.5);
                }
                else if (p < -0.5)
                {
                    do
                    {
                        p += 1.0;
                    }
                    while (p < -0.5);
                }

                return p;
            }
        }
    }
}
