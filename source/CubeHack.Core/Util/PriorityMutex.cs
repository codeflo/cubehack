// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

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
