using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CubeHack.Util;

namespace CubeHack.Game.Behaviors
{
    internal class RunAwayFromPlayerBehavior : Behavior
    {
        public RunAwayFromPlayerBehavior(Entity entity) : base(entity)
        {
            // hier ist nichts zu tun, alles Nötige erledigt der Konstruktor der Oberklasse
        }

        public override void Behave(BehaviorContext context)
        {
            // hier muss das Verhalten des gesteuerten Entities so angepasst werden, dass es vor dem Spieler wegläuft
        }

        public override BehaviorPriority DeterminePriority(BehaviorContext context)
        {
            // hier muss implementiert werden, dass diese Methode einen hohen BehaviorPriority-Wert zurückgibt, wenn ein Spieler sehr nahe am
            // gesteuerten Entity ist
            // Die Implementierung in StopNearPlayerBehavior ist sehr ähnlich und kann als Vorlage dienen
            return BehaviorPriority.NA;
        }

        public override GameDuration MinimumDuration()
        {
            // hier muss implementiert werden, wie lange ein Entity mindestens weglaufen soll
            return new GameDuration(0);
        }
    }
}
