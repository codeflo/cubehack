using CubeHack.Data;
using CubeHack.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    /// <summary>
    /// Models a control strategy for an AI-controlled <see cref="Entity"/>.
    /// </summary>
    internal abstract class Behavior
    {
        private GameDuration _behaviorDuration;

        /// <summary>
        /// The duration this Behavior has been active.
        /// </summary>
        public GameDuration ActiveDuration{ get { return _behaviorDuration; } }

        /// <summary>
        /// Constructs a new Behavior.
        /// </summary>
        public Behavior()
        {
            _behaviorDuration = new GameDuration(0);
        }

        /// <summary>
        /// Adds the given duration to this Behavior's ActiveDuration.
        /// </summary>
        /// <param name="duration">The duration to add.</param>
        public void AddToActiveDuration(GameDuration duration)
        {
            _behaviorDuration += duration;
        }

        /// <summary>
        /// Resets this Behavior.
        /// </summary>
        public void Reset()
        {
            _behaviorDuration = new GameDuration(0);
        }

        /// <summary>
        /// Returns the minimum duration this Behavior shall be active before switching to another Behavior is allowed.
        /// </summary>
        /// <returns>This Behavior's minimum duration</returns>
        public abstract GameDuration MinimumDuration();

        /// <summary>
        /// Determines whether this Behavior is currently applicable for the controlled entity depending on the game's state.
        /// </summary>
        /// <param name="physicsValues">The game's physics settings</param>
        /// <param name="elapsedDuration">The elapsed duration since the last control update</param>
        /// <param name="entity">The controlled entity</param>
        /// <param name="otherEntities">The other entities in the game</param>
        /// <returns></returns>
        public abstract bool CanBehave(PhysicsValues physicsValues, GameDuration elapsedDuration, Entity entity, IEnumerable<Entity> otherEntities);

        /// <summary>
        /// Controls a certain entity depending on the game's state.
        /// </summary>
        /// <param name="physicsValues">The game's physics settings</param>
        /// <param name="elapsedDuration">The elapsed duration since the last control update</param>
        /// <param name="entity">The controlled entity</param>
        /// <param name="otherEntities">The other entities in the game</param>
        public abstract void Behave(PhysicsValues physicsValues, GameDuration elapsedDuration, Entity entity, IEnumerable<Entity> otherEntities);
        
    }
}
