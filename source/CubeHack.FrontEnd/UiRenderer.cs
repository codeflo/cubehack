// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.Ui.Framework.Controls;
using CubeHack.FrontEnd.Ui.Framework.Drawing;
using CubeHack.FrontEnd.Ui.Framework.Input;
using CubeHack.Util;
using System.Collections.Generic;
using System.Globalization;

namespace CubeHack.FrontEnd
{
    internal class UiRenderer
    {
        private readonly Queue<double> _timeMeasurements = new Queue<double>();

        private readonly Canvas _canvas;

        private GameTime _frameTime;

        private UiRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Render(RenderInfo renderInfo, Control control, string status, InputState inputState)
        {
            using (_canvas.SetUpFrame(renderInfo))
            {
                if (inputState.Mouse != null)
                {
                    /* Convert the screen coordinates into virtual canvas coordinates. */
                    inputState.Mouse.Position.X *= _canvas.Width / renderInfo.Width;
                    inputState.Mouse.Position.Y *= _canvas.Height / renderInfo.Height;
                }

                DrawCrossHair(_canvas);

                control.HandleInput(inputState);
                control.Render(_canvas);

                if (status != null)
                {
                    DrawStatus(_canvas, status);
                }

                DrawFps(_canvas);
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
            var style = new Font(30, new Color(1, 1, 1)) { Animation = FontAnimation.Wave };
            canvas.Print(
                style,
                0.5f * (canvas.Width - canvas.MeasureText(style, status)), canvas.Height * 0.5f - 15, status);
        }

        private void DrawFps(Canvas canvas)
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

                canvas.Print(new Font(15, new Color(1, 1, 1)), 5, 5, fpsString);
            }
        }
    }
}
