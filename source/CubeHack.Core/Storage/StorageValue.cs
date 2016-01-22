// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Storage
{
    /// <summary>
    /// Represents a value stored in a save file.
    /// </summary>
    public sealed class StorageValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageValue"/> class from the given byte array.
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
        /// Gets the bytes contained in the value.
        /// <para>
        /// For efficiency reasons, this returns an internal buffer, but you're not allowed to modify the array.
        /// </para>
        /// </summary>
        public byte[] Bytes { get; private set; }

        public static StorageValue Serialize<T>(T instance)
        {
            return new StorageValue(Serialization.Serialize(instance));
        }

        public T Deserialize<T>()
        {
            return Serialization.Deserialize<T>(Bytes);
        }
    }
}
