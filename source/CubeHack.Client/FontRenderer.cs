// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;

namespace CubeHack.Client
{
    internal class FontRenderer
    {
        private readonly Dictionary<char, CharEntry> _charEntries = new Dictionary<char, CharEntry>();

        private bool _isInitializing;
        private bool _isInitialized;
        private int _fontTextureId;

        public void Draw(float x, float y, float width, float height, string text)
        {
            Initialize();
            if (!_isInitialized) return;

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Color3(1f, 1f, 1f);

            GL.BindTexture(TextureTarget.Texture2D, _fontTextureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);

            GL.Begin(PrimitiveType.Quads);

            foreach (var c in text)
            {
                var e = GetCharEntry(c);
                float w = e.Width / e.Height * width;

                if (!char.IsWhiteSpace(c))
                {
                    GL.TexCoord2(e.X, e.Y);
                    GL.Vertex2(x, y);

                    GL.TexCoord2(e.X, e.Y + e.Height);
                    GL.Vertex2(x, y - height);

                    GL.TexCoord2(e.X + e.Width, e.Y + e.Height);
                    GL.Vertex2(x + w, y - height);

                    GL.TexCoord2(e.X + e.Width, e.Y);
                    GL.Vertex2(x + w, y);
                }

                x += w;
            }

            GL.End();
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
        }

        private CharEntry GetCharEntry(char c)
        {
            CharEntry e;
            if (!_charEntries.TryGetValue(c, out e))
            {
                e = _charEntries['?'];
            }

            return e;
        }

        private async void Initialize()
        {
            if (_isInitialized || _isInitializing) return;
            _isInitializing = true;

            _fontTextureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fontTextureId);

            const int fontSize = 32;
            var font = new Font("Arial Black", fontSize);

            int bitmapWidth = 2048;
            int bitmapHeight = 2048;
            float currentX = 0;
            float currentY = 0;

            await TextureHelper.DrawTextureAsync(
                _fontTextureId,
                bitmapWidth,
                bitmapHeight,
                graphics =>
                {
                    float height = -1;

                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    graphics.FillRectangle(Brushes.Transparent, 0, 0, bitmapWidth, bitmapHeight);

                    foreach (char c in GetPrintableChars())
                    {
                        string s = new string(c, 1);

                        if (char.IsWhiteSpace(c))
                        {
                            _charEntries[c] = new CharEntry { Width = graphics.MeasureString("|" + s + "|", font).Width - graphics.MeasureString("||", font).Width };
                            continue;
                        }

                        var stringFormat = new StringFormat(StringFormatFlags.NoWrap);
                        stringFormat.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, 1) });
                        var ranges = graphics.MeasureCharacterRanges(s, font, new RectangleF(0, 0, 10 * fontSize, 10 * fontSize), stringFormat);
                        var rect = ranges[0].GetBounds(graphics);

                        float width = rect.Width;
                        if (height < 0)
                        {
                            height = rect.Height;
                        }

                        if (currentX + width + 1 > bitmapWidth)
                        {
                            currentX = 0;
                            currentY += height;

                            if (currentY + height > bitmapHeight)
                            {
                                // We have run out of space in our image for further characters. This means we need a larger image texture --
                                // though on the other hand, we can't properly handle most Unicode characters anyway, so the whole approach
                                // might need rethinking.
                                break;
                            }
                        }

                        graphics.DrawString(s, font, Brushes.White, currentX - rect.Left, currentY);

                        _charEntries[c] = new CharEntry { X = (currentX - 1) / bitmapWidth, Y = currentY / bitmapHeight, Width = width / bitmapWidth, Height = height / bitmapHeight };
                        currentX += width + 2;
                    }
                },
                null);

            _isInitialized = true;
            _isInitializing = false;
        }

        private IEnumerable<char> GetPrintableChars()
        {
            for (int i = 0; i < 65535; ++i)
            {
                char c = (char)i;

                if (!(char.IsControl(c) || char.IsSurrogate(c) || char.IsWhiteSpace(c)))
                {
                    yield return c;
                }
            }
        }

        private class CharEntry
        {
            public float X, Y, Width, Height;
        }
    }
}
