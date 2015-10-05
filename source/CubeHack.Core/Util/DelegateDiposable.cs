// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Threading;

namespace CubeHack.Util
{
    internal sealed class DelegateDiposable : IDisposable
    {
        private Action _action;

        public DelegateDiposable(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            Action action = Interlocked.Exchange(ref _action, null);
            if (action != null)
            {
                action();
            }
        }
    }
}
