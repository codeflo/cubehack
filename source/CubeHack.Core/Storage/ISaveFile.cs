// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Threading.Tasks;

namespace CubeHack.Storage
{
    public interface ISaveFile : IDisposable
    {
        Task<StorageValue> ReadAsync(StorageKey key);

        void Write(StorageKey key, StorageValue value);
    }
}
