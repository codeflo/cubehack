// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;

namespace CubeHack.FrontEnd.Ui.Framework.Properties
{
    internal class DelegateProperty<T> : IProperty<T>
    {
        private readonly Func<T> _func;

        public DelegateProperty(Func<T> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            _func = func;
        }

        public T Value
        {
            get
            {
                return _func();
            }
        }
    }
}
