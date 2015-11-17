using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CubeHack.Data;
using CubeHack.Util;

namespace CubeHack.Game
{
    internal class RandomWalkBehavior : Behavior
    {
        public override void Behave(PhysicsValues physicsValues, GameDuration elapsedDuration, Entity entity, IEnumerable<Entity> otherEntities)
        {
            if (elapsedDuration.Seconds >= 10.0 * Rng.NextExp())
            {
                entity.PositionData.HAngle = Rng.NextFloat() * 360f;
            }
            else if (elapsedDuration.Seconds > 1.0 * Rng.NextExp())
            {
                entity.PositionData.HAngle += Rng.NextFloat() * 30f - 15f;
            }

            double vz = -0.125 * physicsValues.PlayerMovementSpeed * Math.Cos(entity.PositionData.HAngle * ExtraMath.RadiansPerDegree);
            double vx = -0.125 * physicsValues.PlayerMovementSpeed * Math.Sin(entity.PositionData.HAngle * ExtraMath.RadiansPerDegree);

            entity.PositionData.Velocity.X = vx;
            entity.PositionData.Velocity.Z = vz;
        }

        public override bool CanBehave(PhysicsValues physicsValues, GameDuration elapsedDuration, Entity entity, IEnumerable<Entity> otherEntities)
        {
            return true;
        }

        public override GameDuration MinimumDuration()
        {
            return new GameDuration(0);
        }
    }
}
