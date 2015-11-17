using CubeHack.Data;
using CubeHack.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    internal abstract class Behavior
    {
        private GameDuration _behaviorDuration;

        public GameDuration BehaviorDuration{ get { return _behaviorDuration; } }

        public Behavior()
        {
            _behaviorDuration = new GameDuration(0);
        }

        public void UpdateDuration(GameDuration duration)
        {
            _behaviorDuration += duration;
        }

        public void Reset()
        {
            _behaviorDuration = new GameDuration(0);
        }

        public abstract GameDuration GetMinimumDuration();

        public abstract bool CanBehave(PhysicsValues physicsValues, GameDuration elapsedDuration, Entity entity, IEnumerable<Entity> otherEntities);

        public abstract void Behave(PhysicsValues physicsValues, GameDuration elapsedDuration, Entity entity, IEnumerable<Entity> otherEntities);
        
    }
}
