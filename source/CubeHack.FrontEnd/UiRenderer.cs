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

        [DependencyInjected]
        private UiRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Render(RenderInfo renderInfo, MouseMode mouseMode, Control control, InputState inputState)
        {
            using (_canvas.SetUpFrame(renderInfo, mouseMode))
            {
                if (inputState.Mouse != null)
                {
                    /* Convert the screen coordinates into virtual canvas coordinates. */
                    inputState.Mouse.Position.X *= _canvas.Width / renderInfo.Width;
                    inputState.Mouse.Position.Y *= _canvas.Height / renderInfo.Height;
                }

                control.HandleInput(inputState);
                control.Render(_canvas);

                DrawFps(_canvas);
            }
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
