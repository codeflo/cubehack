// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Runtime.InteropServices;

namespace CubeHack.FrontEnd.Graphics.Engine
{
    internal sealed class ByteBuffer : IDisposable
    {
        private const int _initialCapacity = 1024;

        private IntPtr _data;
        private int _capacity;
        private int _byteCount;

        public ByteBuffer()
        {
        }

        ~ByteBuffer()
        {
            Dispose();
        }

        public int ByteCount => _byteCount;

        public IntPtr Ptr => _data;

        public void Clear()
        {
            _byteCount = 0;
        }

        public void Dispose()
        {
            if (_data != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_data);
                _data = IntPtr.Zero;
            }

            _byteCount = 0;
            _capacity = 0;
        }

        public void EnsureCapacity(int requiredCapacity)
        {
            if (_capacity >= requiredCapacity) return;

            /* Grow at least by a factor of 2 to avoid too many reallocations. */
            _capacity = Math.Max(requiredCapacity, Math.Max(_capacity * 2, _initialCapacity));

            /* Nonsensically, ReAllocHGlobal throws an InsufficientMemoryException if passed IntPtr.Zero as the previous buffer. */
            _data = _data == IntPtr.Zero ? Marshal.AllocHGlobal(_capacity) : Marshal.ReAllocHGlobal(_data, (IntPtr)_capacity);
        }

        public IntPtr Allocate1(int count)
        {
            EnsureCapacity(_byteCount + count);
            var data = _data + _byteCount;
            _byteCount += count;
            return data;
        }

        public IntPtr Allocate2(int count)
        {
            int size = count * 2;
            var alignedByteCount = (_byteCount + 1) & ~1;
            EnsureCapacity(alignedByteCount + size);
            var data = _data + alignedByteCount;
            _byteCount = alignedByteCount + size;
            return data;
        }

        public IntPtr Allocate4(int count)
        {
            int size = count * 4;
            var alignedByteCount = (_byteCount + 3) & ~3;
            EnsureCapacity(alignedByteCount + size);
            var data = _data + alignedByteCount;
            _byteCount = alignedByteCount + size;
            return data;
        }
    }
}
