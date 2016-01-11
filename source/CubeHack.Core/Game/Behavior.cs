// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;

namespace CubeHack.Game
{
    /// <summary>
    /// Models a control strategy for an AI-controlled <see cref="Entity"/>.
    /// </summary>
    internal abstract class Behavior
    {
        private Entity _entity;
        private GameDuration _behaviorDuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Behavior"/> class.
        /// </summary>
        /// <param name="entity">The entity controlled by this behavior.</param>
        public Behavior(Entity entity)
        {
            _entity = entity;
            _behaviorDuration = new GameDuration(0);
        }

        /// <summary>
        /// Gets the Entity controlled by this Behavior.
        /// </summary>
        public Entity Entity
        {
            get { return _entity; }
        }

        /// <summary>
        /// Gets the duration this Behavior has been active.
        /// </summary>
        public GameDuration ActiveDuration
        {
            get { return _behaviorDuration; }
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
        /// Determines this Behavior's priority.
        ///
        /// Values may be:
        /// BehaviorPriority.NA if this Behavior is currently not applicable
        /// BehaviorPriority.Min if this Behavior is applicable, but least important of all Behaviors
        /// BehaviorPriority.Max if this Behavior should be applied in any case
        /// BehaviorPriority.Value(...) if this Behavior should be applied with a given priority value
        /// </summary>
        /// <param name="context">Information related to game and Behavior state</param>
        /// <returns>The Behavior's priority</returns>
        public abstract BehaviorPriority DeterminePriority(BehaviorContext context);

        /// <summary>
        /// Controls a certain entity depending on the game's state.
        /// </summary>
        /// <param name="context">Information related to game and Behavior state</param>
        public abstract void Behave(BehaviorContext context);
    }
}
