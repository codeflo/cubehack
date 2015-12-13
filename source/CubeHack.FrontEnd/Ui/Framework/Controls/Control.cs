// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.FrontEnd.Ui.Framework.Drawing;
using CubeHack.FrontEnd.Ui.Framework.Input;
using System.Collections.Generic;

namespace CubeHack.FrontEnd.Ui.Framework.Controls
{
    internal class Control
    {
        public void Render(Canvas canvas)
        {
            RenderBackground(canvas);
            RenderChildren(canvas);
            RenderForeground(canvas);
        }

        public void HandleKeyPress(KeyPress keyPress)
        {
            OnPreKeyPress(keyPress);
            if (keyPress.IsHandled) return;

            var children = GetChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    child?.HandleKeyPress(keyPress);
                    if (keyPress.IsHandled) return;
                }
            }

            OnKeyPress(keyPress);
        }

        public void HandleInput(InputState input)
        {
            OnPreInput(input);

            var children = GetChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    child?.HandleInput(input);
                }
            }

            OnInput(input);
        }

        public MouseMode HandleGetMouseMode()
        {
            var mouseMode = OnPreGetMouseMode();
            if (mouseMode != MouseMode.Any) return mouseMode;

            var children = GetChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    mouseMode = child?.HandleGetMouseMode() ?? MouseMode.Any;
                    if (mouseMode != MouseMode.Any) return mouseMode;
                }
            }

            return OnGetMouseMode();
        }

        protected virtual MouseMode OnPreGetMouseMode()
        {
            return MouseMode.Any;
        }

        protected virtual MouseMode OnGetMouseMode()
        {
            return MouseMode.Any;
        }

        protected virtual void OnPreKeyPress(KeyPress keyPress)
        {
        }

        protected virtual void OnKeyPress(KeyPress keyPress)
        {
        }

        protected virtual void OnPreInput(InputState input)
        {
        }

        protected virtual void OnInput(InputState input)
        {
        }

        protected virtual IEnumerable<Control> GetChildren()
        {
            return null;
        }

        protected virtual void RenderForeground(Canvas canvas)
        {
        }

        protected virtual void RenderBackground(Canvas canvas)
        {
        }

        private void RenderChildren(Canvas canvas)
        {
            var children = GetChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    child?.Render(canvas);
                }
            }
        }
    }
}
