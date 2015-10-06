// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System.Diagnostics;

namespace CubeHack.Util
{
    public class PrecisionTimer
    {
        private static readonly float _oneOverTicksPerSecond = 1.0f / (float)Stopwatch.Frequency;

        private readonly Stopwatch _stopwatch;
        private long _zeroTicks;

        public PrecisionTimer()
        {
            _stopwatch = Stopwatch.StartNew();
            SetZero();
        }

        public float ElapsedTime
        {
            get
            {
                return (_stopwatch.ElapsedTicks - _zeroTicks) * _oneOverTicksPerSecond;
            }
        }

        public float SetZero()
        {
            long elapsedTicks = _stopwatch.ElapsedTicks;
            float elapsedTime = (elapsedTicks - _zeroTicks) * _oneOverTicksPerSecond;
            _zeroTicks = elapsedTicks;
            return elapsedTime;
        }
    }
}
