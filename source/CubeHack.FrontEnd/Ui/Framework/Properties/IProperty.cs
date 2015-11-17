// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.FrontEnd.Ui.Framework.Properties
{
    internal interface IProperty<out T>
    {
        T Value { get; }
    }
}
