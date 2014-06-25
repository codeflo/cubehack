// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Data;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Client
{
    static class TextureAtlas
    {
        const int _textureSize = 256;

        static int _count;
        static int _size;

        static int _textureId;
        static TextureEntry[] _textureEntries;

        public static int TextureId
        {
            get
            {
                return _textureId;
            }
        }

        public static TextureEntry GetTextureEntry(int index)
        {
            return _textureEntries[index];
        }

        public static void Bind()
        {
            if (_textureId == 0)
            {
                _textureId = GL.GenTexture();
            }

            GL.BindTexture(TextureTarget.Texture2D, _textureId);
        }

        public static void Build(List<Texture> textures)
        {
            Bind();

            _count = textures.Count;

            // Find a texture size that fits all cube textures.
            for (_size = 1; _size * _size < _count; _size += _size) { }

            _textureEntries = new TextureEntry[_count];
            float tf = 1f / _textureSize;

            TextureHelper.DrawTexture(
                _size * _textureSize,
                _size * _textureSize,
                graphics =>
                {
                    int x = 0, y = 0;
                    for (int i = 0; i < _count; ++i)
                    {
                        ++x;
                        if (x == _size)
                        {
                            x = 0;
                            ++y;
                        }

                        DrawTexture(graphics, textures[i], x, y);
                        _textureEntries[i] = new TextureEntry { X0 = x * tf, Y0 = (y + _textureSize) * tf, X1 = (x + _textureSize) * tf, Y1 = y * tf };
                    }
                });
        }

        static void DrawTexture(Graphics graphics, Texture texture, int x, int y)
        {
            var color = new CubeHack.Data.Color(texture.Color);
            graphics.FillRectangle(BrushFromColor(color), x, y, x + _textureSize, y + _textureSize);
        }

        static Brush BrushFromColor(CubeHack.Data.Color c)
        {
            return new SolidBrush(System.Drawing.Color.FromArgb(
                IntFromFloatColor(c.R),
                IntFromFloatColor(c.G),
                IntFromFloatColor(c.B)));
        }

        static int IntFromFloatColor(float v)
        {
            return (int)(v * 255f);
        }

        public struct TextureEntry
        {
            public float X0, Y0, X1, Y1;
        }
    }
}
