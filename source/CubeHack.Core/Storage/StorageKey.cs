// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Util;
using System.IO;

namespace CubeHack.Storage
{
    /// <summary>
    /// Represents a key to be used to store and retrieve data in a save file.
    /// </summary>
    public sealed class StorageKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageKey"/> class from the given byte array.
        /// <para>
        /// For efficiency reasons, this doesn't create a copy of the byte array, which means that you're not allowed
        /// to modify the array afterwards.
        /// </para>
        /// </summary>
        /// <param name="bytes">The bytes to store in the key.</param>
        public StorageKey(byte[] bytes)
        {
            Bytes = bytes;
        }

        /// <summary>
        /// Gets the bytes contained in the key.
        /// <para>
        /// For efficiency reasons, this returns an internal buffer, but you're not allowed to modify the array.
        /// </para>
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// Serializes the given list of objects and returns a StorageKey containing their representation.
        /// </summary>
        /// <param name="parts">The parts of the key.</param>
        /// <returns>A StorageKey.</returns>
        public static StorageKey Get(params object[] parts)
        {
            using (var memoryStream = new MemoryStream())
            {
                if (parts != null)
                {
                    ProtoBuf.Serializer.Serialize(memoryStream, parts);
                }

                return new StorageKey(memoryStream.ToArray());
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCalculator.Value[Bytes];
        }

        public override bool Equals(object obj)
        {
            var other = obj as StorageKey;
            if (other == null) return false;

            if (Bytes == null || Bytes.Length == 0) return other.Bytes == null || other.Bytes.Length == 0;
            if (Bytes.Length != other.Bytes.Length) return false;

            for (int i = 0; i < Bytes.Length; ++i)
            {
                if (Bytes[i] != other.Bytes[i]) return false;
            }

            return true;
        }
    }
}
