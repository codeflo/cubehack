// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;

namespace CubeHack.FrontEnd.Ui.Framework.Properties
{
    internal static class DelegateProperty
    {
        public static DelegateProperty<T> Get<T>(Func<T> func)
        {
            return new DelegateProperty<T>(func);
        }
    }
}
