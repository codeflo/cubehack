using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CubeHack.Data;
using CubeHack.Util;

namespace CubeHack.Game
{
    class StopNearPlayerBehavior : Behavior
    {
        public override void Behave(PhysicsValues physicsValues, GameDuration elapsedDuration, Entity entity, IEnumerable<Entity> otherEntities)
        {
            entity.PositionData.Velocity.X = 0;
            entity.PositionData.Velocity.Z = 0;
        }

        public override bool CanBehave(PhysicsValues physicsValues, GameDuration elapsedDuration, Entity entity, IEnumerable<Entity> otherEntities)
        {
            foreach(Entity other in otherEntities)
            {
                if (other.IsAiControlled) continue;

                var offset = entity.PositionData.Position - other.PositionData.Position;

                if (Math.Abs(offset.Length) < 4) return true;
            }

            return false;
        }

        public override GameDuration MinimumDuration()
        {
            return new GameDuration(2);
        }
    }
}
