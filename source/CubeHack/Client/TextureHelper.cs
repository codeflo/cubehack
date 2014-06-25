// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Client
{
    static class TextureHelper
    {
        public static void DrawTexture(int width, int height, Action<Graphics> drawAction)
        {
            using (var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    drawAction(graphics);
                }

                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                try
                {
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
