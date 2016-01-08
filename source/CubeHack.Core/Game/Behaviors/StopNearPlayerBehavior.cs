// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;

namespace CubeHack.Game.Behaviors
{
    internal class StopNearPlayerBehavior : Behavior
    {
        public StopNearPlayerBehavior(Entity entity) : base(entity)
        {
        }

        public override void Behave(BehaviorContext context)
        {
            Entity.PositionData.Velocity.X = 0;
            Entity.PositionData.Velocity.Z = 0;
        }

        public override BehaviorPriority DeterminePriority(BehaviorContext context)
        {
            foreach (Entity other in context.otherEntities)
            {
                if (other.IsAiControlled) continue;

                var offset = Entity.PositionData.Placement.Pos - other.PositionData.Placement.Pos;

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
