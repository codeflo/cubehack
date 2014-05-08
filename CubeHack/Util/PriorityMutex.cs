// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CubeHack.Util
{
    sealed class PriorityMutex
    {
        readonly object _lowMutex = new object();
        readonly object _priorityLockWaitMutex = new object();
        readonly object _actualMutex = new object();
        int _priorityLockRequestCount = 0;

        public IDisposable TakeLock()
        {
            Monitor.Enter(_lowMutex);
            lock (_priorityLockWaitMutex)
            {
                Monitor.Enter(_actualMutex);
            }

            return new DelegateDiposable(ReleaseLock);
        }

        void ReleaseLock()
        {
            Monitor.Exit(_actualMutex);
            Monitor.Exit(_lowMutex);
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

        void ReleasePriorityLock()
        {
            Monitor.Exit(_actualMutex);

            if (Interlocked.Decrement(ref _priorityLockRequestCount) == 0)
            {
                Monitor.Exit(_priorityLockWaitMutex);
            }
        }
    }
}
