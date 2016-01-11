// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Geometry;
using CubeHack.Util;
using System;

namespace CubeHack.Game.Behaviors
{
    internal class RandomWalkBehavior : Behavior
    {
        public RandomWalkBehavior(Entity entity)
            : base(entity)
        {
        }

        public override void Behave(BehaviorContext context)
        {
            var horizontal = Entity.PositionData.Placement.Orientation.Horizontal;
            if (context.ElapsedDuration.Seconds >= 10.0 * Rng.NextExp())
            {
                horizontal = Rng.NextDouble() * 2 * Math.PI;
            }
            else if (context.ElapsedDuration.Seconds > 1.0 * Rng.NextExp())
            {
                horizontal += (Rng.NextDouble() * 2 - 1) * 0.25;
            }

            var orientation = Entity.PositionData.Placement.Orientation = new EntityOrientation(horizontal, 0);
            var velocity = -0.125 * context.PhysicsValues.PlayerMovementSpeed * (EntityOffset)orientation;
            Entity.PositionData.Velocity.X = velocity.X;
            Entity.PositionData.Velocity.Z = velocity.Z;
        }

        public override BehaviorPriority DeterminePriority(BehaviorContext context)
        {
            return BehaviorPriority.Min;
        }

        public override GameDuration MinimumDuration()
        {
            return new GameDuration(0);
        }
    }
}
