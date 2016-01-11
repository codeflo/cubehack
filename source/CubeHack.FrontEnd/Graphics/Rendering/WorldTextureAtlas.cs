// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.Graphics.Engine;
using CubeHack.Game;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CubeHack.FrontEnd.Graphics.Rendering
{
    internal sealed class WorldTextureAtlas : IDisposable
    {
        public const int TextureSizeBits = 8;
        public const int TextureSize = 1 << TextureSizeBits;

        private readonly List<Texture> _textures = new List<Texture>();

        private int _size;
        private int _count;
        private int _textureId;
        private TextureEntry[] _textureEntries;

        [DependencyInjected]
        public WorldTextureAtlas()
        {
        }

        public int TextureId
        {
            get
            {
                return _textureId;
            }
        }

        public TextureEntry GetTextureEntry(int index)
        {
            return _textureEntries[index];
        }

        public void Dispose()
        {
            if (_textureId != 0)
            {
                GL.DeleteTexture(_textureId);
            }
        }

        public void Bind()
        {
            if (_textureId == 0) _textureId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, TextureSizeBits - 1);
        }

        public async Task SetModAsync(ModData mod)
        {
            Bind();

            _textures.Clear();
            _textures.Add(null);
            foreach (var texture in mod.Materials.Select(m => m.Texture))
            {
                texture.Index = _textures.Count;
                _textures.Add(texture);
            }

            _count = _textures.Count;

            // Find a texture size that fits all material textures.
            for (_size = 1; _size * _size < _count; _size += _size) { }

            _textureEntries = new TextureEntry[_count];

            double tf = (double)ushort.MaxValue / _size;
            double to = (double)ushort.MaxValue / (_size * TextureSize);

            await TextureHelper.DrawTextureAsync(
                _textureId,
                _size * TextureSize,
                _size * TextureSize,
                graphics =>
                {
                    int x = 0, y = 0;
                    for (int i = 0; i < _count; ++i)
                    {
                        var texture = _textures[i];

                        System.Drawing.Brush brush;
                        if (texture == null)
                        {
                            brush = System.Drawing.Brushes.White;
                        }
                        else
                        {
                            var color = new Color(texture.Color);
                            brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb((int)(color.R * 255), (int)(color.G * 255), (int)(color.B * 255)));
                        }

                        var x0 = x * TextureSize;
                        var y0 = y * TextureSize;

                        graphics.FillRectangle(brush, new System.Drawing.Rectangle(x0, y0, TextureSize, TextureSize));

                        _textureEntries[i] = new TextureEntry
                        {
                            PixelX0 = x0,
                            PixelY0 = y0,
                            PixelX1 = x0 + TextureSize,
                            PixelY1 = y0 + TextureSize,
                            X0 = (ushort)(x * tf + to),
                            Y0 = (ushort)(y * tf + to),
                            X1 = (ushort)((x + 1) * tf - to),
                            Y1 = (ushort)((y + 1) * tf - to),
                        };

                        ++x;
                        if (x == _size)
                        {
                            x = 0;
                            ++y;
                        }
                    }
                },
                null);

            RunTextureGeneration();
        }

        private async void RunTextureGeneration()
        {
            for (int i = 0; i < _count; ++i)
            {
                var texture = _textures[i];
                if (texture == null) continue;

                var entry = _textureEntries[i];

                await TextureHelper.DrawSubTextureAsync(
                    _textureId,
                    entry.PixelX0,
                    entry.PixelY0,
                    TextureSize,
                    TextureSize,
                    null,
                    bitmapData => TextureGenerator.DrawTexture(bitmapData, texture, 0, 0));
            }
        }

        public struct TextureEntry
        {
            public int PixelX0;
            public int PixelY0;
            public int PixelX1;
            public int PixelY1;

            public ushort X0;
            public ushort Y0;
            public ushort X1;
            public ushort Y1;
        }
    }
}
