// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System.Collections.Generic;
using System.IO;

namespace CubeHack.Storage
{
    /// <summary>
    /// Represents a value stored in a save file.
    /// </summary>
    public sealed class StorageValue
    {
        /// <summary>
        /// Creates a new StorageValue from the given byte array.
        /// <para>
        /// For efficiency reasons, this doesn't create a copy of the byte array, which means that you're not allowed
        /// to modify the array afterwards.
        /// </para>
        /// </summary>
        /// <param name="bytes">The bytes to store in the value.</param>
        public StorageValue(byte[] bytes)
        {
            Bytes = bytes;
        }

        /// <summary>
        /// Returns the bytes contained in the value.
        /// <para>
        /// For efficiency reasons, this returns an internal buffer, but you're not allowed to modify the array.
        /// </para>
        /// </summary>
        public byte[] Bytes { get; private set; }

        public static StorageValue Serialize<T>(T instance)
        {
            using (var memoryStream = new MemoryStream())
            {
                if (!EqualityComparer<T>.Default.Equals(instance, default(T)))
                {
                    ProtoBuf.Serializer.Serialize(memoryStream, instance);
                }

                return new StorageValue(memoryStream.ToArray());
            }
        }

        public T Deserialize<T>()
        {
            if (Bytes == null || Bytes.Length == 0) return default(T);

            using (var memoryStream = new MemoryStream(Bytes))
            {
                return ProtoBuf.Serializer.Deserialize<T>(memoryStream);
            }
        }
    }
}
