// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.FrontEnd.Ui.Framework.Properties
{
    internal static class Property
    {
        public static Property<T> Get<T>(T value)
        {
            return new Property<T>(value);
        }
    }
}
