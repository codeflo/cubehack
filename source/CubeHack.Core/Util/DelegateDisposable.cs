// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Threading;

namespace CubeHack.Util
{
    public sealed class DelegateDisposable : IDisposable
    {
        private Action _action;

        public DelegateDisposable(Action action)
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
