// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Threading;

namespace CubeHack.Util
{
    public static class ThreadPoolConfiguration
    {
        static ThreadPoolConfiguration()
        {
            /* Use more aggressive defaults for the number of threads in the ThreadPool. */

            int preferredThreads = Math.Min(2 * Environment.ProcessorCount, 64);

            int workerThreads, completionPortThreads;

            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            workerThreads = Math.Max(workerThreads, preferredThreads);
            completionPortThreads = Math.Max(completionPortThreads, preferredThreads);
            ThreadPool.SetMaxThreads(workerThreads, completionPortThreads);

            ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
            workerThreads = Math.Max(workerThreads, preferredThreads);
            completionPortThreads = Math.Max(completionPortThreads, preferredThreads);
            ThreadPool.SetMinThreads(workerThreads, completionPortThreads);
        }

        public static void Init()
        {
        }
    }
}
