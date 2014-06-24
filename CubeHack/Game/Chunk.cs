// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    class Chunk
    {
        public const int Bits = 5;
        public const int Size = 1 << Bits;

        private ushort[] _data;

        public ulong ContentHash { get; private set; }

        public ushort this[int x, int y, int z]
        {
            get
            {
                int index = GetIndex(x, y, z);
                return _data == null ? (ushort)0 : _data[index];
            }

            set
            {
                int index = GetIndex(x, y, z);
                if (_data == null)
                {
                    _data = new ushort[Size * Size * Size];
                }

                ContentHash = ContentHash - GetCubeHash(index, _data[index]) + GetCubeHash(index, value);
                _data[index] = value;
            }
        }

        private int GetIndex(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= Size || y >= Size || z >= Size)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (x * Size + y) * Size + z;
        }

        private ulong GetCubeHash(int index, ushort value)
        {
            return (((ulong)index << 1) | 1) * value;
        }
    }
}
