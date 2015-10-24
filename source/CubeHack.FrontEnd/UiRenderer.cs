// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.UiFramework;
using CubeHack.Util;
using System.Collections.Generic;
using System.Globalization;

namespace CubeHack.FrontEnd
{
    internal static class UiRenderer
    {
        private static readonly Queue<double> _timeMeasurements = new Queue<double>();

        private static GameTime _frameTime;

        public static void Render(RenderInfo renderInfo, bool mouseLookActive, string status)
        {
            using (var canvas = new Canvas(renderInfo))
            {
                if (mouseLookActive)
                {
                    DrawCrossHair(canvas);
                }
                else
                {
                    if (!mouseLookActive || status != null)
                    {
                        canvas.DrawRectangle(new Color(0, 0, 0, 0.5f), 0, 0, canvas.Width, canvas.Height);
                        DrawStatus(canvas, status ?? "Continue");
                    }
                }

                DrawFps(canvas);
            }
        }

        private static void DrawCrossHair(Canvas canvas)
        {
            var color = new Color(1, 1, 1);
            var x = canvas.Width * 0.5f;
            var y = canvas.Height * 0.5f;

            var l = 4;
            var w = 0.7f;

            canvas.DrawRectangle(color, x - w, y - l, x + w, y + l);
            canvas.DrawRectangle(color, x - l, y - w, x + l, y + w);
        }

        private static void DrawStatus(Canvas canvas, string status)
        {
            canvas.Print(new Color(1, 1, 1), 20, 0.5f * (canvas.Width - canvas.MeasureText(20, status)), canvas.Height - 100, status);
        }

        private static void DrawFps(Canvas canvas)
        {
            var frameDuration = GameTime.Update(ref _frameTime);
            if (_timeMeasurements.Count >= 50)
            {
                _timeMeasurements.Dequeue();
            }

            _timeMeasurements.Enqueue(frameDuration.Seconds);
            double totalTime = 0f;
            foreach (float time in _timeMeasurements)
            {
                totalTime += time;
            }

            if (totalTime > 0)
            {
                double fps = _timeMeasurements.Count / totalTime;

                string fpsString = string.Format(CultureInfo.InvariantCulture, "{0:0}FPS", fps);

                canvas.Print(new Color(1, 1, 1), 15, 5, 5, fpsString);
            }
        }
    }
}
