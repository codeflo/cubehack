// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System.Threading.Tasks;

namespace CubeHack.Storage
{
    /// <summary>
    /// A dummy <see cref="ISaveFile"/> implementation that doesn't save anything.
    /// </summary>
    public class NullSaveFile : ISaveFile
    {
        public static readonly NullSaveFile Instance = new NullSaveFile();

        public void Dispose()
        {
        }

        public Task<StorageValue> ReadAsync(StorageKey key)
        {
            return null;
        }

        public void Write(StorageKey key, StorageValue value)
        {
        }
    }
}
