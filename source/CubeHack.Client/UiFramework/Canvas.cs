﻿// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.Util;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace CubeHack.Client.UiFramework
{
    /// <summary>
    /// A canvas is used to perform 2D (user interface) drawing.
    /// <para>
    /// The canvas uses a virtual coordinate system that's more like traditional UI frameworks and less like OpenGL.
    /// The upper left corner is (0, 0), and the lower right corner is (Width, Height), measured in virtual pixels.
    /// </para>
    /// <para>
    /// Virtual pixels are scaled from screen pixels to establish a coordinate system that's independent of the physical
    /// resolution. The diagonal of the screen is always 1024 virtual pixels long; this roughly corresponds to a
    /// virtual screen size of 800*600 (depending on the aspect ratio).
    /// </para>
    /// </summary>
    internal class Canvas : IDisposable
    {
        private const float _uiDiagonal = 1024f;

        public Canvas(RenderInfo renderInfo)
        {
            SetUpScreen(renderInfo);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public float Width { get; private set; }

        public float Height { get; private set; }

        public void Dispose()
        {
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
        }

        public float MeasureText(float lineHeight, string text)
        {
            if (text == null) return 0;

            float x = 0;
            foreach (var c in text)
            {
                x += GameApp.Instance.CharMap.GetCharWidth(lineHeight, c);
            }

            return x;
        }

        public void Print(Color color, float lineHeight, float x, float y, string text)
        {
            if (text == null) return;

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, GameApp.Instance.CharMap.TextureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.LinearMipmapLinear);

            GL.Color4(color.R, color.G, color.B, color.A);

            foreach (var c in text)
            {
                x += GameApp.Instance.CharMap.PrintChar(lineHeight, x, y, c);
            }

            GL.Disable(EnableCap.Texture2D);
        }

        public void DrawRectangle(Color color, float x0, float y0, float x1, float y1)
        {
            GL.Color4(color.R, color.G, color.B, color.A);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(x0, y1);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x1, y0);
            GL.Vertex2(x0, y0);
            GL.End();
        }

        private void SetUpScreen(RenderInfo renderInfo)
        {
            float diagonalFactor = (float)(_uiDiagonal / Math.Sqrt(ExtraMath.Square(renderInfo.Width) + ExtraMath.Square(renderInfo.Height)));

            Width = renderInfo.Width * diagonalFactor;
            Height = renderInfo.Height * diagonalFactor;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            var modelViewMatrix = Matrix4.Zero;

            modelViewMatrix.M11 = 2.0f / Width;
            modelViewMatrix.M41 = -1.0f;
            modelViewMatrix.M22 = -2.0f / Height;
            modelViewMatrix.M42 = 1.0f;
            modelViewMatrix.M33 = 1;
            modelViewMatrix.M44 = 1;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelViewMatrix);
        }
    }
}
