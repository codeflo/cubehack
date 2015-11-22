﻿// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.Ui.Framework;
using CubeHack.FrontEnd.Ui.Menu;
using CubeHack.Util;
using System.Collections.Generic;
using System.Globalization;

namespace CubeHack.FrontEnd
{
    internal static class UiRenderer
    {
        private static readonly Queue<double> _timeMeasurements = new Queue<double>();

        private static GameTime _frameTime;

        private static MainMenu _mainMenu = new MainMenu();

        public static void Render(RenderInfo renderInfo, bool mouseLookActive, string status)
        {
            using (var canvas = new Canvas(renderInfo))
            {
                if (mouseLookActive)
                {
                    DrawCrossHair(canvas);
                }

                _mainMenu.IsVisible = !mouseLookActive && status == null;
                _mainMenu.Render(canvas);

                if (!mouseLookActive || status != null)
                {
                    DrawStatus(canvas, status);
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
            var style = new FontStyle(30, new Color(1, 1, 1)) { Animation = FontAnimation.Wave };
            canvas.Print(
                style,
                0.5f * (canvas.Width - canvas.MeasureText(style, status)), canvas.Height * 0.5f - 15, status);
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

                canvas.Print(new FontStyle(15, new Color(1, 1, 1)) { IsBold = true }, 5, 5, fpsString);
            }
        }
    }
}
