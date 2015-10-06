// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

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
            action?.Invoke();
        }
    }
}
