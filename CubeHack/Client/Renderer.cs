// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Client
{
    class Renderer
    {
        const float _viewingAngle = 100f;

        public static void Render(GameConnection gameConnection, int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            GL.ClearColor(0.5f, 0.6f, 0.9f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            SetProjectionMatrix(width, height);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Rotate(gameConnection.Position.VAngle, 1, 0, 0);
            GL.Rotate(gameConnection.Position.HAngle, 0, -1, 0);
            GL.Translate(-gameConnection.Position.X, -gameConnection.Position.Y - 0.9, -gameConnection.Position.Z);

            GL.Disable(EnableCap.Texture2D);
            DrawEntity(0, 0, -5);
            if (gameConnection.EntityPositions != null)
            {
                float dt = gameConnection.TimeSinceGameEvent;

                foreach (var position in gameConnection.EntityPositions)
                {
                    GL.PushMatrix();
                    GL.Translate(position.X, position.Y, position.Z);
                    GL.Rotate(position.HAngle + 180, 0, 1, 0);
                    DrawEntity(position.VX * dt, position.VY * dt, position.VZ * dt);
                    GL.PopMatrix();
                }
            }

            GL.Enable(EnableCap.Texture2D);
            TextureAtlas.Bind();
            DrawCube(5, 0, -5);
        }

        static void SetProjectionMatrix(float width, float height)
        {
            float dw = width;
            float dh = height;
            float dd = (float)Math.Sqrt(dw * dw + dh * dh);
            float df = 1.0f / (float)Math.Tan(_viewingAngle / 180.0 * Math.PI * 0.5);

            float yScale = df * dd / dh;
            float xScale = df * dd / dw;
            float near = 0.1f;
            float far = 100.0f;
            float length = far - near;

            Matrix4 projectionMatrix = Matrix4.Zero;

            projectionMatrix.M11 = xScale;
            projectionMatrix.M22 = yScale;
            projectionMatrix.M33 = (-far - near) / length;
            projectionMatrix.M34 = -1;
            projectionMatrix.M43 = -2 * near * far / length;
            projectionMatrix.M44 = 0;

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projectionMatrix);
        }

        static void DrawEntity(float x, float y, float z)
        {
            GL.Begin(PrimitiveType.Quads);

            GL.Color3(0.9f, 0.9f, 0.9f);
            GL.Vertex3(x - 0.4f, y, z + 0.4f);
            GL.Vertex3(x + 0.4f, y, z + 0.4f);
            GL.Vertex3(x + 0.4f, y + 1.75f, z + 0.4f);
            GL.Vertex3(x - 0.4f, y + 1.75f, z + 0.4f);

            GL.Color3(0.7f, 0.8f, 0.9f);

            GL.Vertex3(x + 0.4f, y, z + 0.4f);
            GL.Vertex3(x + 0.4f, y, z - 0.4f);
            GL.Vertex3(x + 0.4f, y + 1.75f, z - 0.4f);
            GL.Vertex3(x + 0.4f, y + 1.75f, z + 0.4f);

            GL.Vertex3(x + 0.4f, y, z - 0.4f);
            GL.Vertex3(x - 0.4f, y, z - 0.4f);
            GL.Vertex3(x - 0.4f, y + 1.75f, z - 0.4f);
            GL.Vertex3(x + 0.4f, y + 1.75f, z - 0.4f);

            GL.Vertex3(x - 0.4f, y, z - 0.4f);
            GL.Vertex3(x - 0.4f, y, z + 0.4f);
            GL.Vertex3(x - 0.4f, y + 1.75f, z + 0.4f);
            GL.Vertex3(x - 0.4f, y + 1.75f, z - 0.4f);

            GL.End();
        }

        static void DrawCube(float x, float y, float z)
        {
            var textureEntry = TextureAtlas.GetTextureEntry(0);

            GL.Begin(PrimitiveType.Quads);

            GL.Color3(0.67f, 0.67f, 0.67f);

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.25f, y - 0.25f, z + 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.25f, y - 0.25f, z + 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.25f, y + 0.25f, z + 0.25f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.25f, y + 0.25f, z + 0.25f);

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x + 0.25f, y - 0.25f, z + 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.25f, y - 0.25f, z - 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.25f, y + 0.25f, z - 0.25f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x + 0.25f, y + 0.25f, z + 0.25f);

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x + 0.25f, y - 0.25f, z - 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x - 0.25f, y - 0.25f, z - 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x - 0.25f, y + 0.25f, z - 0.25f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x + 0.25f, y + 0.25f, z - 0.25f);

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.25f, y - 0.25f, z - 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x - 0.25f, y - 0.25f, z + 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x - 0.25f, y + 0.25f, z + 0.25f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.25f, y + 0.25f, z - 0.25f);

            GL.Color3(1f, 1f, 1f);

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.25f, y + 0.25f, z + 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.25f, y + 0.25f, z + 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.25f, y + 0.25f, z - 0.25f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.25f, y + 0.25f, z - 0.25f);

            GL.Color3(0.33f, 0.33f, 0.33f);

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.25f, y - 0.25f, z + 0.25f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.25f, y - 0.25f, z - 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.25f, y - 0.25f, z - 0.25f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.25f, y - 0.25f, z + 0.25f);

            GL.End();
        }
    }
}
