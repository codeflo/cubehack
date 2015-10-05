// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Data;
using CubeHack.Util;
using System;

namespace CubeHack.Game
{
    internal static class Ai
    {
        public static void Control(PhysicsValues physicsValues, double elapsedTime, Entity entity)
        {
            if (elapsedTime >= 10.0 * Rng.NextExp())
            {
                entity.PositionData.HAngle = Rng.NextFloat() * 360f;
            }
            else if (elapsedTime > 1.0 * Rng.NextExp())
            {
                entity.PositionData.HAngle += Rng.NextFloat() * 30f - 15f;
            }

            double vz = -0.125 * physicsValues.PlayerMovementSpeed * Math.Cos(entity.PositionData.HAngle * ExtraMath.RadiansPerDegree);
            double vx = -0.125 * physicsValues.PlayerMovementSpeed * Math.Sin(entity.PositionData.HAngle * ExtraMath.RadiansPerDegree);

            entity.PositionData.Velocity.X = vx;
            entity.PositionData.Velocity.Z = vz;
        }
    }
}
