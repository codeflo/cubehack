// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace CubeHack.Client.UiFramework
{
    /// <summary>
    /// Manages the font texture.
    /// </summary>
    internal class CharMap
    {
        private const string _fontName = "AndikaNewBasic-R.ttf";

        private static readonly string _fontDir = Path.Combine(
                    Path.GetDirectoryName(typeof(CharMap).Assembly.Location),
            "fonts");

        private static readonly PrivateFontCollection _fontCollection = new PrivateFontCollection();
        private readonly Dictionary<char, CharEntry> _charEntries = new Dictionary<char, CharEntry>();
        private bool _isInitializing;
        private bool _isInitialized;
        private int _fontTextureId;

        public int TextureId => _fontTextureId;

        public float GetCharWidth(float lineHeight, char c)
        {
            Initialize();
            if (!_isInitialized) return 0;

            return GetCharEntry(c).InnerWidth * lineHeight;
        }

        public float PrintChar(float lineHeight, float x, float y, char c)
        {
            Initialize();
            if (!_isInitialized) return 0;

            GL.Begin(PrimitiveType.Quads);

            var e = GetCharEntry(c);

            if (!char.IsWhiteSpace(c))
            {
                GL.TexCoord2(e.TextureX, e.TextureY);
                GL.Vertex2(x - e.BoxLeft * lineHeight, y);

                GL.TexCoord2(e.TextureX, e.TextureY + e.TextureHeight);
                GL.Vertex2(x - e.BoxLeft * lineHeight, y + lineHeight);

                GL.TexCoord2(e.TextureX + e.TextureWidth, e.TextureY + e.TextureHeight);
                GL.Vertex2(x + e.BoxRight * lineHeight, y + lineHeight);

                GL.TexCoord2(e.TextureX + e.TextureWidth, e.TextureY);
                GL.Vertex2(x + e.BoxRight * lineHeight, y);

                GL.End();
            }

            return e.InnerWidth * lineHeight;
        }

        private static void LoadFonts()
        {
            string fontPath = Path.Combine(_fontDir, _fontName);
            using (var fontStream = File.Open(fontPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                System.IntPtr data = Marshal.AllocCoTaskMem((int)fontStream.Length);
                try
                {
                    byte[] fontdata = new byte[fontStream.Length];
                    fontStream.Read(fontdata, 0, (int)fontStream.Length);
                    Marshal.Copy(fontdata, 0, data, (int)fontStream.Length);
                    _fontCollection.AddMemoryFont(data, (int)fontStream.Length);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(data);
                }
            }
        }

        private static RectangleF MeasureRectangle(Graphics graphics, int fontSize, Font font, string s)
        {
            var stringFormat = new StringFormat(StringFormatFlags.NoWrap);
            stringFormat.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, 1) });
            var ranges = graphics.MeasureCharacterRanges(s, font, new RectangleF(0, 0, 10 * fontSize, 10 * fontSize), stringFormat);
            var rect = ranges[0].GetBounds(graphics);
            return rect;
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

            LoadFonts();

            _fontTextureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fontTextureId);

            const int fontSize = 64;
            var font = new Font(_fontCollection.Families[0], fontSize);

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
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    graphics.FillRectangle(Brushes.Transparent, 0, 0, bitmapWidth, bitmapHeight);

                    var xRectangle = MeasureRectangle(graphics, fontSize, font, "x");
                    float xWidth = xRectangle.Width;
                    float xHeight = xRectangle.Height;

                    foreach (char c in GetPrintableChars())
                    {
                        string s = new string(c, 1);

                        if (char.IsWhiteSpace(c))
                        {
                            _charEntries[c] = new CharEntry { TextureWidth = graphics.MeasureString("X" + s + "X", font).Width - graphics.MeasureString("xx", font).Width };
                            continue;
                        }

                        RectangleF charRectangle = MeasureRectangle(graphics, fontSize, font, s);
                        float charWidth = charRectangle.Width, charHeight = charRectangle.Height;

                        if (currentX + charWidth + xWidth > bitmapWidth)
                        {
                            currentX = 0;
                            currentY += xHeight;

                            if (currentY + xHeight > bitmapHeight)
                            {
                                // We have run out of space in our image for further characters. This means we need a larger image texture --
                                // though on the other hand, we can't properly handle most Unicode characters anyway, so the whole approach
                                // might need rethinking.
                                break;
                            }
                        }

                        graphics.DrawString(s, font, Brushes.White, currentX + 0.5f * xWidth - charRectangle.Left, currentY);

                        float measuredCharWidth = graphics.MeasureString("X" + s + "X", font).Width - graphics.MeasureString("XX", font).Width;

                        _charEntries[c] = new CharEntry
                        {
                            TextureX = currentX / bitmapWidth,
                            TextureY = currentY / bitmapHeight,
                            TextureWidth = (charWidth + xWidth) / bitmapWidth,
                            TextureHeight = charHeight / bitmapHeight,

                            BoxLeft = 0.5f * xWidth / xHeight,
                            BoxRight = (charWidth + 0.5f * xWidth) / xHeight,
                            InnerWidth = measuredCharWidth / xHeight,
                        };

                        currentX += charWidth + xWidth;
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
            public float TextureX, TextureY, TextureWidth, TextureHeight;
            public float BoxLeft;
            public float BoxRight;
            public float InnerWidth;
        }
    }
}
