// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System.Diagnostics;

namespace CubeHack.Util
{
    /// <summary>
    /// Represents the precise time of a game event, measured with a high precision timer.
    /// </summary>
    public struct GameTime
    {
        private static readonly long _start = Stopwatch.GetTimestamp();
        private static readonly double _tickFrequency = Stopwatch.Frequency;
        private static readonly double _tickDuration = 1.0 / _tickFrequency;

        private long _elapsedTicks;

        public static GameTime Now()
        {
            return new GameTime { _elapsedTicks = Stopwatch.GetTimestamp() - _start };
        }

        public static GameDuration operator -(GameTime a, GameTime b)
        {
            return new GameDuration(_tickDuration * (a._elapsedTicks - b._elapsedTicks));
        }

        public static GameTime operator +(GameTime b, GameDuration d)
        {
            return new GameTime { _elapsedTicks = b._elapsedTicks + (long)(_tickFrequency * d.Seconds) };
        }

        public static GameTime operator +(GameDuration d, GameTime b)
        {
            return new GameTime { _elapsedTicks = (long)(_tickFrequency * d.Seconds) + b._elapsedTicks };
        }

        public static GameTime operator -(GameTime b, GameDuration d)
        {
            return new GameTime { _elapsedTicks = b._elapsedTicks - (long)(_tickFrequency * d.Seconds) };
        }

        public static bool operator ==(GameTime a, GameTime b)
        {
            return a._elapsedTicks == b._elapsedTicks;
        }

        public static bool operator !=(GameTime a, GameTime b)
        {
            return a._elapsedTicks != b._elapsedTicks;
        }

        public static bool operator >(GameTime a, GameTime b)
        {
            return a._elapsedTicks > b._elapsedTicks;
        }

        public static bool operator >=(GameTime a, GameTime b)
        {
            return a._elapsedTicks >= b._elapsedTicks;
        }

        public static bool operator <(GameTime a, GameTime b)
        {
            return a._elapsedTicks < b._elapsedTicks;
        }

        public static bool operator <=(GameTime a, GameTime b)
        {
            return a._elapsedTicks <= b._elapsedTicks;
        }

        /// <summary>
        /// Updates the referenced frameTime to the current time, and returns the precise duration that has passed since then.
        /// </summary>
        /// <param name="frameTime">The frame time to update.</param>
        /// <returns>The duration between the old value of frameTime and the current time.</returns>
        public static GameDuration Update(ref GameTime frameTime)
        {
            var now = Now();
            var elapsedDuration = now - frameTime;
            frameTime = now;
            return elapsedDuration;
        }

        public override bool Equals(object obj)
        {
            return obj is GameTime && (_elapsedTicks == ((GameTime)obj)._elapsedTicks);
        }

        public override int GetHashCode()
        {
            return HashCalculator.Value[_elapsedTicks];
        }
    }
}
