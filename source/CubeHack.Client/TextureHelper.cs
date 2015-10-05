// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace CubeHack.Client
{
    internal static class TextureHelper
    {
        public static void DrawTexture(int width, int height, Action<Graphics> drawAction, Action<BitmapData> bitmapAction)
        {
            using (var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                if (drawAction != null)
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        drawAction(graphics);
                    }
                }

                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                try
                {
                    if (bitmapAction != null)
                    {
                        bitmapAction(bitmapData);
                    }

                    GL.TexImage2D(
                        TextureTarget.Texture2D,
                        0,
                        PixelInternalFormat.Rgba,
                        bitmapData.Width,
                        bitmapData.Height,
                        0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                        PixelType.UnsignedByte,
                        bitmapData.Scan0);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
        }
    }
}
