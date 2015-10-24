// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace CubeHack.FrontEnd
{
    internal static class TextureHelper
    {
        public static async Task DrawTextureAsync(int textureId, int width, int height, Action<Graphics> drawAction, Action<BitmapData> bitmapAction)
        {
            using (var computedBitmap = await Task.Run(() => new ComputedBitmap(width, height, drawAction, bitmapAction)))
            {
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                GL.TexImage2D(
                        TextureTarget.Texture2D,
                        0,
                        PixelInternalFormat.Rgba,
                        computedBitmap.BitmapData.Width,
                        computedBitmap.BitmapData.Height,
                        0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                        PixelType.UnsignedByte,
                        computedBitmap.BitmapData.Scan0);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        private class ComputedBitmap : IDisposable
        {
            private readonly Bitmap _bitmap;
            private readonly BitmapData _bitmapData;

            public ComputedBitmap(int width, int height, Action<Graphics> drawAction, Action<BitmapData> bitmapAction)
            {
                _bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                if (drawAction != null)
                {
                    using (var graphics = Graphics.FromImage(_bitmap))
                    {
                        drawAction(graphics);
                    }
                }

                _bitmapData = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                if (bitmapAction != null)
                {
                    bitmapAction(_bitmapData);
                }
            }

            public BitmapData BitmapData
            {
                get
                {
                    return _bitmapData;
                }
            }

            public void Dispose()
            {
                try
                {
                    _bitmap.UnlockBits(_bitmapData);
                }
                finally
                {
                    _bitmap.Dispose();
                }
            }
        }
    }
}
