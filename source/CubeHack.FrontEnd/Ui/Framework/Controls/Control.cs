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

        public bool HandleKeyDown(Key key)
        {
            if (OnPreKeyDown(key)) return true;

            var children = GetChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child?.HandleKeyDown(key) ?? false) return true;
                }
            }

            return OnKeyDown(key);
        }

        public bool HandleTextInput(string text)
        {
            if (OnPreTextInput(text)) return true;

            var children = GetChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child?.HandleTextInput(text) ?? false) return true;
                }
            }

            return OnTextInput(text);
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

        protected virtual bool OnPreKeyDown(Key key)
        {
            return false;
        }

        protected virtual bool OnKeyDown(Key key)
        {
            return false;
        }

        protected virtual bool OnPreTextInput(string text)
        {
            return false;
        }

        protected virtual bool OnTextInput(string text)
        {
            return false;
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
