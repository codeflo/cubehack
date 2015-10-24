// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CubeHack.FrontEnd
{
    internal class TextureAtlas
    {
        public const int TextureSizeBits = 8;
        public const int TextureSize = 1 << TextureSizeBits;

        private readonly List<Texture> _textures = new List<Texture>();

        private int _count;
        private int _size;

        private int _textureId;
        private TextureEntry[] _textureEntries;

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

        public void Bind()
        {
            if (_textureId == 0)
            {
                _textureId = GL.GenTexture();
            }

            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, TextureSizeBits - 1);
        }

        public int Register(Texture texture)
        {
            int index = _count;
            texture.Index = index;
            _textures.Add(texture);
            ++_count;
            return index;
        }

        public async Task BuildAsync()
        {
            Bind();

            // Find a texture size that fits all cube textures.
            for (_size = 1; _size * _size < _count; _size += _size) { }

            _textureEntries = new TextureEntry[_count];
            float tf = 1f / _size;
            float to = 1f / (_size * TextureSize);

            await TextureHelper.DrawTextureAsync(
                _textureId,
                _size * TextureSize,
                _size * TextureSize,
                null,
                bitmapData =>
                {
                    int x = 0, y = 0;
                    for (int i = 0; i < _count; ++i)
                    {
                        TextureGenerator.DrawTexture(bitmapData, _textures[i], x * TextureSize, y * TextureSize);
                        _textureEntries[i] = new TextureEntry { X0 = x * tf + to, Y0 = y * tf + to, X1 = (x + 1) * tf - to, Y1 = (y + 1) * tf - to };

                        ++x;
                        if (x == _size)
                        {
                            x = 0;
                            ++y;
                        }
                    }
                });
        }

        public struct TextureEntry
        {
            public float X0, Y0, X1, Y1;
        }
    }
}
