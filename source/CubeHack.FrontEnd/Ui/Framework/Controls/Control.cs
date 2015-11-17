// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

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
