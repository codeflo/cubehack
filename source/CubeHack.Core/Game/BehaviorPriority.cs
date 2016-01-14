// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;

namespace CubeHack.Game
{
    internal struct BehaviorPriority
    {
        private int _priority;

        private BehaviorPriority(int priority)
        {
            if (priority < -1)
            {
                throw new ArgumentOutOfRangeException("priority", "priority may not be smaller than -1");
            }

            _priority = priority;
        }

        public static BehaviorPriority NA
        {
            get
            {
                return new BehaviorPriority(-1);
            }
        }

        public static BehaviorPriority Min
        {
            get
            {
                return new BehaviorPriority(0);
            }
        }

        public static BehaviorPriority Max
        {
            get
            {
                return new BehaviorPriority(int.MaxValue);
            }
        }

        public static bool operator ==(BehaviorPriority prio1, BehaviorPriority prio2)
        {
            return prio1._priority == prio2._priority;
        }

        public static bool operator !=(BehaviorPriority prio1, BehaviorPriority prio2)
        {
            return !(prio1 == prio2);
        }

        public static bool operator <(BehaviorPriority prio1, BehaviorPriority prio2)
        {
            return prio1._priority < prio2._priority;
        }

        public static bool operator >(BehaviorPriority prio1, BehaviorPriority prio2)
        {
            return prio1._priority > prio2._priority;
        }

        public static double operator /(BehaviorPriority prio1, BehaviorPriority prio2)
        {
            if (prio2 == BehaviorPriority.Min) return prio1._priority;

            return (double)prio1._priority / (double)prio2._priority;
        }

        public static BehaviorPriority operator +(BehaviorPriority prio1, BehaviorPriority prio2)
        {
            if (prio1 == BehaviorPriority.Min)
            {
                return prio2;
            }

            if (prio2 == BehaviorPriority.Min)
            {
                return prio1;
            }

            if (prio1 == BehaviorPriority.Max || prio2 == BehaviorPriority.Max)
            {
                return BehaviorPriority.Max;
            }

            return new BehaviorPriority(prio1._priority + prio2._priority);
        }

        public static BehaviorPriority Value(int priority)
        {
            return new BehaviorPriority(priority);
        }

        public override bool Equals(object other)
        {
            if (!(other is BehaviorPriority))
            {
                return false;
            }

            return this == ((BehaviorPriority)other);
        }

        public override int GetHashCode()
        {
            return _priority;
        }
    }
}
