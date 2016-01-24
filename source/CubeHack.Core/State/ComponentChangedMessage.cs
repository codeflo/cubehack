// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.State
{
    internal class ComponentChangedMessage<TContainer, TComponent>
    {
        public TContainer Owner;

        public TComponent Component;

        public ComponentChangedMessage()
        {
        }

        public ComponentChangedMessage(TContainer owner, TComponent component)
        {
            Owner = owner;
            Component = component;
        }
    }
}
