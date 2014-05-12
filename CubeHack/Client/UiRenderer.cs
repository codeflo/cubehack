// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Util;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Client
{
    static class UiRenderer
    {
        static readonly PrecisionTimer _frameTimer = new PrecisionTimer();
        static readonly Queue<float> _timeMeasurements = new Queue<float>();

        public static void Render(float width, float height)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            DrawFps(width, height);
        }

        static void DrawFps(float width, float height)
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
                FontRenderer.Draw(-1, 1, 0.04f, 0.04f * width / height, fpsString);
            }
        }
    }
}
