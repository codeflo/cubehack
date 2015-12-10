// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.FrontEnd.Ui.Framework
{
    internal struct Rectangle
    {
        public double Left;

        public double Top;

        public double Right;

        public double Bottom;

        private Rectangle(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public double Width
        {
            get { return Right - Left; }
            set { Right = Left + value; }
        }

        public double Height
        {
            get { return Bottom - Top; }
            set { Bottom = Top + value; }
        }

        public static Rectangle FromSize(double left, double top, double width, double height)
        {
            return new Rectangle(left, top, left + width, top + height);
        }

        public static Rectangle FromEdges(double left, double top, double right, double bottom)
        {
            return new Rectangle(left, top, right, bottom);
        }

        public bool Contains(Point point)
        {
            return Left <= point.X && Top <= point.Y && point.X <= Right && point.Y <= Bottom;
        }
    }
}
