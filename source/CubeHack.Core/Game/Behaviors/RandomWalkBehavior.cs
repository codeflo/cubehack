// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;
using System;

namespace CubeHack.Game.Behaviors
{
    internal class RandomWalkBehavior : Behavior
    {
        public RandomWalkBehavior(Entity entity) : base(entity)
        {
        }

        public override void Behave(BehaviorContext context)
        {
            if (context.elapsedDuration.Seconds >= 10.0 * Rng.NextExp())
            {
                Entity.PositionData.HAngle = Rng.NextFloat() * 360f;
            }
            else if (context.elapsedDuration.Seconds > 1.0 * Rng.NextExp())
            {
                Entity.PositionData.HAngle += Rng.NextFloat() * 30f - 15f;
            }

            double vz = -0.125 * context.physicsValues.PlayerMovementSpeed * Math.Cos(Entity.PositionData.HAngle * ExtraMath.RadiansPerDegree);
            double vx = -0.125 * context.physicsValues.PlayerMovementSpeed * Math.Sin(Entity.PositionData.HAngle * ExtraMath.RadiansPerDegree);

            Entity.PositionData.Velocity.X = vx;
            Entity.PositionData.Velocity.Z = vz;
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
