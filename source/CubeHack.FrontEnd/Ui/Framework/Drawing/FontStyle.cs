// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;

namespace CubeHack.FrontEnd.Ui.Framework
{
    internal class FontStyle
    {
        public FontStyle(float size)
        {
            Size = size;
        }

        public FontStyle(float height, Color color)
        {
            Size = height;
            Color = color;
        }

        public float Size { get; set; }

        public Color Color { get; set; } = new Color(1, 1, 1);

        public bool IsBold { get; set; } = false;

        public FontAnimation Animation { get; set; } = FontAnimation.None;
    }
}
