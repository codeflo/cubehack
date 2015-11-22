using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            this._priority = priority;
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

        public override bool Equals(Object other)
        {
            if(!(other is BehaviorPriority))
            {
                return false;
            }
            return this == ((BehaviorPriority)other);
        }

        public override int GetHashCode()
        {
            return _priority;
        }

        public static BehaviorPriority Value(int priority)
        {
            return new BehaviorPriority(priority);
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
               

    }
}
