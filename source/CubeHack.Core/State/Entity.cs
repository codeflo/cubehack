// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.State
{
    public sealed class Entity : Container<Entity>
    {
        public Entity(Universe universe)
            : base(universe.Messenger)
        {
            Universe = universe;
            universe.AddEntity(this);
        }

        public Universe Universe { get; private set; }

        public void Destroy()
        {
            Universe = null;
            Universe.RemoveEntity(this);
        }
    }
}
