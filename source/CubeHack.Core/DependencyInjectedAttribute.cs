// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;

namespace CubeHack
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class DependencyInjectedAttribute : Attribute
    {
    }
}
