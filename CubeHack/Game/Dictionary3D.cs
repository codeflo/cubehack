// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    class Dictionary3D<T>
    {
        private Dictionary<Tuple<int, int, int>, T> _data = new Dictionary<Tuple<int, int, int>, T>();

        public T this[int x, int y, int z]
        {
            get
            {
                T ret;
                _data.TryGetValue(Tuple.Create(x, y, z), out ret);
                return ret;
            }

            set
            {
                _data[Tuple.Create(x, y, z)] = value;
            }
        }
    }
}
