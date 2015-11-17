// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.Util;
using System;
using System.Collections.Generic;

namespace CubeHack.Game
{
    internal static class Ai
    {
        /// <summary>
        /// Active Behavior per Entity
        /// </summary>
        private static Dictionary<Entity, Behavior> _activeBehavior = new Dictionary<Entity, Behavior>();

        /// <summary>
        /// All possible Behaviors per Entity
        /// </summary>
        private static Dictionary<Entity, List<Behavior>> _possibleBehaviors = new Dictionary<Entity, List<Behavior>>();

        public static void Control(PhysicsValues physicsValues, GameDuration elapsedDuration, Entity entity, IEnumerable<Entity> otherEntities)
        {   
            // initialize Behavior stores if necessary         
            if(!_activeBehavior.ContainsKey(entity))
            {
                Behavior defaultBehavior = InitializeBehaviors(entity);

                _activeBehavior.Add(entity, defaultBehavior);
            }

            Behavior current = _activeBehavior[entity];

            // update the duration the active Behavior has been running
            current.AddToActiveDuration(elapsedDuration);

            // if the minimum running duration of the current Behavior has passed, look for another applicable Behavior
            if(current.ActiveDuration > current.MinimumDuration())
            {
                foreach(Behavior b in _possibleBehaviors[entity])
                {
                    // look for *other* Behaviors that are currently applicable
                    if (!current.Equals(b) && b.CanBehave(physicsValues, elapsedDuration, entity, otherEntities))
                    {
                        // reset the current Behavior, as it might become active again later
                        current.Reset();

                        // change active Behavior to new one
                        _activeBehavior[entity] = b;
                        current = b;

                        //Console.WriteLine("Entity {0} switched to behavior {1}", entity, b);
                    }
                }
            }

            // let the active Behavior control the entity
            current.Behave(physicsValues, elapsedDuration, entity, otherEntities);
        }

        /// <summary>
        /// Initializes the possible Behaviors for an (AI-controlled) Entity.
        /// </summary>
        /// <param name="entity">The Entity to initialize</param>
        /// <returns>Initial Behavior the Entity should have</returns>
        private static Behavior InitializeBehaviors(Entity entity)
        {
            var list = new List<Behavior>() { new RandomWalkBehavior(), new StopNearPlayerBehavior() };
            _possibleBehaviors.Add(entity, list);

            return list[0];
        }
    }
}
