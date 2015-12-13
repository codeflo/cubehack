// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.Ui.Framework.Drawing;
using CubeHack.FrontEnd.Ui.Framework.Input;
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

        private bool _wasLeftMouseButtonPressed;

        public event Action Click;

        public IProperty<string> Text { get; set; }

        public IProperty<bool> IsEnabled { get; set; }

        public IProperty<float> Left { get; set; }

        public IProperty<float> Top { get; set; }

        public bool IsHovered { get; private set; }

        public bool IsPressed { get; private set; }

        protected override void OnInput(InputState input)
        {
            /*
             * This should behave like standard button on Windows.
             *
             * If you press and release the mouse button INSIDE the Button area,
             * raise a click event.
             *
             * If you press the mouse button, move OUTSIDE the Button area, and release
             * it there, DON'T raise a click event.
             *
             * If you press the mouse button outside the Button area, don't raise anything.
             *
             * Also, if the mouse is inside the Button area, set IsHovered.
             */

            IsHovered = input.Mouse != null && Left != null && Top != null &&
                Rectangle.FromSize(Left.Value, Top.Value, Width, Height).Contains(input.Mouse.Position);

            if (!input.Mouse.LeftButtonPressed)
            {
                if (IsPressed)
                {
                    if (IsHovered)
                    {
                        Click?.Invoke();
                    }

                    IsPressed = false;
                }
            }
            else if (IsHovered && !_wasLeftMouseButtonPressed)
            {
                IsPressed = true;
            }

            _wasLeftMouseButtonPressed = input.Mouse.LeftButtonPressed;
        }

        protected override void RenderBackground(Canvas canvas)
        {
            var left = Left?.Value ?? 0;
            var top = Top?.Value ?? 0;

            var color = IsHovered ? (IsPressed ? new Color(0.0f, 0.2f, 0.5f, 0.75f) : new Color(0.3f, 0.6f, 1.0f, 0.75f)) : new Color(0.1f, 0.4f, 0.7f, 0.75f);

            canvas.DrawRectangle(new Color(0.1f, 0.1f, 0.1f, 0.5f), left, top, left + Width, top + Height);
            canvas.DrawRectangle(color, left, top, left + Width, top + Height);
        }

        protected override void RenderForeground(Canvas canvas)
        {
            var left = Left?.Value ?? 0;
            var top = Top?.Value ?? 0;
            var text = Text?.Value;

            var style = new Font(_fonstSize);

            style.Color = new Color(0, 0, 0, 0.5f);
            canvas.Print(style, left + _shadowOffset + 0.5f * (Width - canvas.MeasureText(style, text)), top + _marginTop + _shadowOffset, text);

            style.Color = new Color(1, 1, 1);
            canvas.Print(style, left + 0.5f * (Width - canvas.MeasureText(style, text)), top + _marginTop, text);
        }
    }
}
