// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.Util;
using System.Collections.Generic;

namespace CubeHack.Game
{
    /// <summary>
    /// Information about the game's context that is passed to <see cref="Behavior"/>s during AI control.
    /// </summary>
    internal struct BehaviorContext
    {
        public PhysicsValues PhysicsValues;

        public GameDuration ElapsedDuration;

        public IEnumerable<Entity> OtherEntities;

        public BehaviorContext(PhysicsValues physicsValues, GameDuration elapsedDuration, IEnumerable<Entity> otherEntities)
        {
            PhysicsValues = physicsValues;
            ElapsedDuration = elapsedDuration;
            OtherEntities = otherEntities;
        }
    }
}
