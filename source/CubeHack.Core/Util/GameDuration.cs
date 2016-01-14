// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;

namespace CubeHack.Util
{
    /// <summary>
    /// Represents a duration (or time span) between two <see cref="GameTime"/> instances.
    /// </summary>
    [ProtoContract]
    public struct GameDuration
    {
        public static readonly GameDuration Zero = default(GameDuration);

        public GameDuration(double seconds)
        {
            Seconds = seconds;
        }

        [ProtoMember(1)]
        public double Seconds { get; set; }

        public static GameDuration operator +(GameDuration a, GameDuration b)
        {
            return new GameDuration(a.Seconds + b.Seconds);
        }

        public static GameDuration operator -(GameDuration a, GameDuration b)
        {
            return new GameDuration(a.Seconds - b.Seconds);
        }

        public static GameDuration operator *(GameDuration a, double f)
        {
            return new GameDuration(a.Seconds * f);
        }

        public static GameDuration operator *(double f, GameDuration a)
        {
            return new GameDuration(f * a.Seconds);
        }

        public static GameDuration operator /(GameDuration a, double f)
        {
            return new GameDuration(a.Seconds / f);
        }

        public static bool operator ==(GameDuration a, GameDuration b)
        {
            return a.Seconds == b.Seconds;
        }

        public static bool operator !=(GameDuration a, GameDuration b)
        {
            return a.Seconds != b.Seconds;
        }

        public static bool operator >(GameDuration a, GameDuration b)
        {
            return a.Seconds > b.Seconds;
        }

        public static bool operator >=(GameDuration a, GameDuration b)
        {
            return a.Seconds >= b.Seconds;
        }

        public static bool operator <(GameDuration a, GameDuration b)
        {
            return a.Seconds < b.Seconds;
        }

        public static bool operator <=(GameDuration a, GameDuration b)
        {
            return a.Seconds <= b.Seconds;
        }

        public static GameDuration FromSeconds(double seconds)
        {
            return new GameDuration(seconds);
        }

        public override bool Equals(object obj)
        {
            return obj is GameDuration && (Seconds == ((GameDuration)obj).Seconds);
        }

        public override int GetHashCode()
        {
            return HashCalculator.Value[Seconds];
        }
    }
}
