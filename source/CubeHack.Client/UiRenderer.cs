// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CubeHack.Client
{
    internal static class UiRenderer
    {
        private static readonly PrecisionTimer _frameTimer = new PrecisionTimer();
        private static readonly Queue<float> _timeMeasurements = new Queue<float>();

        public static void Render(float width, float height, bool mouseLookActive, string status)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Disable(EnableCap.DepthTest);

            if (!mouseLookActive)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Color4(0, 0, 0, 0.5f);
                GL.Begin(PrimitiveType.Quads);
                GL.Vertex2(-1f, -1f);
                GL.Vertex2(1f, -1f);
                GL.Vertex2(1f, 1f);
                GL.Vertex2(-1f, 1f);
                GL.End();
                GL.Disable(EnableCap.Blend);

                GameApp.Instance.FontRenderer.Draw(-0.85f, -0.6f, 0.06f, 0.06f * width / height, status ?? "Continue");
            }

            DrawFps(width, height);

            if (mouseLookActive)
            {
                DrawCrossHair(width, height);
            }
        }

        private static void DrawCrossHair(float width, float height)
        {
            float d = (float)Math.Sqrt(width * width + height * height);
            float w = (float)height / d;
            float h = (float)width / d;
            float dwh = d / (width * height);

            float n = 4 * dwh;
            float m = 24 * dwh;

            GL.Color3(1f, 1f, 1f);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(-m * w, -n * h);
            GL.Vertex2(m * w, -n * h);
            GL.Vertex2(m * w, n * h);
            GL.Vertex2(-m * w, n * h);

            GL.Vertex2(-n * w, -m * h);
            GL.Vertex2(n * w, -m * h);
            GL.Vertex2(n * w, m * h);
            GL.Vertex2(-n * w, m * h);
            GL.End();
        }

        private static void DrawFps(float width, float height)
        {
            float elapsedTime = _frameTimer.SetZero();
            if (_timeMeasurements.Count >= 50)
            {
                _timeMeasurements.Dequeue();
            }

            _timeMeasurements.Enqueue(elapsedTime);
            float totalTime = 0f;
            foreach (float time in _timeMeasurements)
            {
                totalTime += time;
            }

            if (totalTime > 0)
            {
                float fps = _timeMeasurements.Count / totalTime;

                string fpsString = string.Format(CultureInfo.InvariantCulture, "{0:0.0} fps", fps);
                GameApp.Instance.FontRenderer.Draw(-1, 1, 0.04f, 0.04f * width / height, fpsString);
            }
        }
    }
}
