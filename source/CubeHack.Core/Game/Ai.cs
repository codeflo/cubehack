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
        private static Dictionary<Entity, Behavior> _activeBehavior = new Dictionary<Entity, Behavior>();

        private static Dictionary<Entity, List<Behavior>> _possibleBehaviors = new Dictionary<Entity, List<Behavior>>();

        public static void Control(PhysicsValues physicsValues, GameDuration elapsedDuration, Entity entity, IEnumerable<Entity> otherEntities)
        {            
            if(!_activeBehavior.ContainsKey(entity))
            {
                Behavior defaultBehavior = InitializeBehaviors(entity);

                _activeBehavior.Add(entity, defaultBehavior);
            }

            Behavior current = _activeBehavior[entity];

            current.UpdateDuration(elapsedDuration);

            if(current.BehaviorDuration > current.GetMinimumDuration())
            {
                foreach(Behavior b in _possibleBehaviors[entity])
                {
                    if (!current.Equals(b) && b.CanBehave(physicsValues, elapsedDuration, entity, otherEntities))
                    {
                        current.Reset();
                        _activeBehavior.Add(entity, b);

                        current = b;
                        
                    }
                }
            }

            current.Behave(physicsValues, elapsedDuration, entity, otherEntities);
        }

        private static Behavior InitializeBehaviors(Entity entity)
        {
            var list = new List<Behavior>() { new RandomWalkBehavior() };
            _possibleBehaviors.Add(entity, list);

            return list[0];
        }
    }
}
