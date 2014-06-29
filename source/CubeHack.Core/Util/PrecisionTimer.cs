// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Util
{
    public class PrecisionTimer
    {
        static readonly float _oneOverTicksPerSecond = 1.0f / (float)Stopwatch.Frequency;

        readonly Stopwatch _stopwatch;
        long _zeroTicks;

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
