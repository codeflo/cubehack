// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;

namespace CubeHack.State
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    internal sealed class ComponentAttribute : Attribute
    {
        public ComponentAttribute(string name)
        {
            Name = name;
        }

        private string Name { get; }
    }
}
