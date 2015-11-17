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
            throw new NotImplementedException();
        }

        public override bool CanBehave(PhysicsValues physicsValues, GameDuration elapsedDuration, Entity entity, IEnumerable<Entity> otherEntities)
        {
            throw new NotImplementedException();
        }

        public override GameDuration GetMinimumDuration()
        {
            throw new NotImplementedException();
        }
    }
}
