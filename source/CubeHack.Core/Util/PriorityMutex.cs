// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Threading;

namespace CubeHack.Util
{
    internal sealed class PriorityMutex
    {
        private readonly object _lowMutex = new object();
        private readonly object _priorityLockWaitMutex = new object();
        private readonly object _actualMutex = new object();
        private int _priorityLockRequestCount = 0;

        public IDisposable TakeLock()
        {
            Monitor.Enter(_lowMutex);
            lock (_priorityLockWaitMutex)
            {
                Monitor.Enter(_actualMutex);
            }

            return new DelegateDiposable(ReleaseLock);
        }

        public IDisposable TakePriorityLock()
        {
            if (Interlocked.Increment(ref _priorityLockRequestCount) == 1)
            {
                Monitor.Enter(_priorityLockWaitMutex);
            }

            Monitor.Enter(_actualMutex);

            return new DelegateDiposable(ReleasePriorityLock);
        }

        private void ReleaseLock()
        {
            Monitor.Exit(_actualMutex);
            Monitor.Exit(_lowMutex);
        }

        private void ReleasePriorityLock()
        {
            Monitor.Exit(_actualMutex);

            if (Interlocked.Decrement(ref _priorityLockRequestCount) == 0)
            {
                Monitor.Exit(_priorityLockWaitMutex);
            }
        }
    }
}
