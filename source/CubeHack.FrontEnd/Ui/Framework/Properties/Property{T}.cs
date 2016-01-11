// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.FrontEnd.Ui.Framework.Properties
{
    internal class Property<T> : IProperty<T>
    {
        public Property()
        {
        }

        public Property(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}
