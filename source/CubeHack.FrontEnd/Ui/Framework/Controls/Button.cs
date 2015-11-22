// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.Ui.Framework.Properties;
using System;

namespace CubeHack.FrontEnd.Ui.Framework.Controls
{
    internal class Button : Control
    {
        public const float Width = 120;
        public const float Height = 30;

        private const float _fonstSize = 25;
        private const float _marginTop = 1;
        private const float _shadowOffset = 1.5f;

        public event Action Click;

        public IProperty<string> Text { get; set; }

        public IProperty<bool> IsEnabled { get; set; }

        public IProperty<float> Left { get; set; }

        public IProperty<float> Top { get; set; }

        protected override void RenderBackground(Canvas canvas)
        {
            var left = Left?.Value ?? 0;
            var top = Top?.Value ?? 0;
            canvas.DrawRectangle(new Color(0.1f, 0.1f, 0.1f, 0.5f), left, top, left + Width, top + Height);
            canvas.DrawRectangle(new Color(0.1f, 0.4f, 0.7f, 0.75f), left, top, left + Width, top + Height);
        }

        protected override void RenderForeground(Canvas canvas)
        {
            var left = Left?.Value ?? 0;
            var top = Top?.Value ?? 0;
            var text = Text?.Value;

            var style = new FontStyle(_fonstSize) { IsBold = true };

            style.Color = new Color(0, 0, 0, 0.5f);
            canvas.Print(style, left + _shadowOffset + 0.5f * (Width - canvas.MeasureText(style, text)), top + _marginTop + _shadowOffset, text);

            style.Color = new Color(1, 1, 1);
            canvas.Print(style, left + 0.5f * (Width - canvas.MeasureText(style, text)), top + _marginTop, text);
        }
    }
}
