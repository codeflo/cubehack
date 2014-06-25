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
    sealed class DelegateDiposable : IDisposable
    {
        Action _action;

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
