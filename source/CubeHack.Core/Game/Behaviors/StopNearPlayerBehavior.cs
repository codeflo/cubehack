// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.State;
using CubeHack.Util;

namespace CubeHack.Game.Behaviors
{
    internal class StopNearPlayerBehavior : Behavior
    {
        public StopNearPlayerBehavior(Entity entity)
            : base(entity)
        {
        }

        public override void Behave(BehaviorContext context, PositionComponent position)
        {
            position.Velocity.X = 0;
            position.Velocity.Z = 0;
        }

        public override BehaviorPriority DeterminePriority(BehaviorContext context)
        {
            foreach (Entity other in Entity.Universe.GetEntities())
            {
                if (other.Has<AiComponent>()) continue;

                var offset = Entity.Get<PositionComponent>().Placement.Pos - other.Get<PositionComponent>().Placement.Pos;

                if (offset.Length < 4)
                {
                    return BehaviorPriority.Value(4);
                }
            }

            return BehaviorPriority.NA;
        }

        public override GameDuration MinimumDuration()
        {
            return new GameDuration(2);
        }
    }
}
