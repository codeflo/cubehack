// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.Ui.Framework.Controls;
using CubeHack.FrontEnd.Ui.Framework.Drawing;
using CubeHack.FrontEnd.Ui.Framework.Input;

namespace CubeHack.FrontEnd.Ui.Hud
{
    internal sealed class HudControl : Control
    {
        [DependencyInjected]
        public HudControl()
        {
        }

        protected override void RenderForeground(Canvas canvas)
        {
            if (canvas.MouseMode == MouseMode.Grabbed) DrawCrossHair(canvas);
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
    }
}
