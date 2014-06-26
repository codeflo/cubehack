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
        const int _textureSizeBits = 8;
        const int _textureSize = 1 << _textureSizeBits;

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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, _textureSizeBits - 1);
        }

        public static void Build(List<Texture> textures)
        {
            Bind();

            _count = textures.Count;

            // Find a texture size that fits all cube textures.
            for (_size = 1; _size * _size < _count; _size += _size) { }

            _textureEntries = new TextureEntry[_count];
            float tf = 1f / _size;

            TextureHelper.DrawTexture(
                _size * _textureSize,
                _size * _textureSize,
                graphics =>
                {
                    int x = 0, y = 0;
                    for (int i = 0; i < _count; ++i)
                    {
                        DrawTexture(graphics, textures[i], x * _textureSize, y * _textureSize);
                        _textureEntries[i] = new TextureEntry { X0 = x * tf, Y0 = y * tf, X1 = (x + 1) * tf, Y1 = (y + 1) * tf };

                        ++x;
                        if (x == _size)
                        {
                            x = 0;
                            ++y;
                        }
                    }
                });
        }

        static void DrawTexture(Graphics graphics, Texture texture, int x, int y)
        {
            var color = new CubeHack.Data.Color(texture.Color);
            graphics.FillRectangle(BrushFromColor(color), x, y, _textureSize, _textureSize);
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
