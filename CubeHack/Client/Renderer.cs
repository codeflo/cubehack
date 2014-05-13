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
            //GL.Enable(EnableCap.CullFace);

            SetProjectionMatrix(width, height);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Rotate(gameConnection.PlayerAngleV, 1, 0, 0);
            GL.Rotate(gameConnection.PlayerAngleH, 0, -1, 0);
            GL.Translate(-gameConnection.PlayerX, -gameConnection.PlayerY - 0.9, -gameConnection.PlayerZ);

            DrawEntity(0, 0, -5);
            if (gameConnection.Entities != null)
            {
                float dt = gameConnection.TimeSinceGameEvent;

                foreach (var e in gameConnection.Entities)
                {
                    DrawEntity(e.X + e.VX * dt, e.Y + e.VY * dt, e.Z + e.VZ * dt);
                }
            }
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
            GL.Color3(0.9f, 0.9f, 0.9f);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(x - 0.5f, y, z + 0.5f);
            GL.Vertex3(x + 0.5f, y, z + 0.5f);
            GL.Vertex3(x + 0.5f, y + 1.8f, z + 0.5f);
            GL.Vertex3(x - 0.5f, y + 1.8f, z + 0.5f);
            GL.End();
        }
    }
}
