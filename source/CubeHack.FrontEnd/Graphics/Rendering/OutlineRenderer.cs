// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using OpenTK.Graphics.OpenGL;
using System;

namespace CubeHack.FrontEnd.Graphics.Rendering
{
    internal sealed class OutlineRenderer
    {
        private readonly Lazy<Shader> _postProcessShader = new Lazy<Shader>(() => Shader.Load("CubeHack.FrontEnd.Shaders.PostProcess"));
        private readonly Lazy<int> _depthBufferTexture = new Lazy<int>(() => GL.GenTexture());

        private int _depthBufferWidth;
        private int _depthBufferHeight;

        [DependencyInjected]
        public OutlineRenderer()
        {
        }

        public void RenderOutlines(RenderInfo renderInfo)
        {
            GL.Flush();
            GL.UseProgram(_postProcessShader.Value.Id);

            GL.BindTexture(TextureTarget.Texture2D, _depthBufferTexture.Value);

            if (renderInfo.Width != _depthBufferWidth || renderInfo.Height != _depthBufferHeight)
            {
                _depthBufferWidth = renderInfo.Width;
                _depthBufferHeight = renderInfo.Height;
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, _depthBufferWidth, _depthBufferHeight, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            }

            GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, _depthBufferWidth, _depthBufferHeight);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 0);
            GL.Vertex3(-1, -1, 0);
            GL.TexCoord2(1, 0);
            GL.Vertex3(1, -1, 0);
            GL.TexCoord2(1, 1);
            GL.Vertex3(1, 1, 0);
            GL.TexCoord2(0, 1);
            GL.Vertex3(-1, 1, 0);
            GL.End();

            GL.UseProgram(0);
        }
    }
}
