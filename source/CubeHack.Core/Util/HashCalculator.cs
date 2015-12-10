// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Security.Cryptography;

namespace CubeHack.Util
{
    /// <summary>
    /// A fast, convenient way to write good implementations of GetHashCode(). It's important for us to
    /// have good hash codes because we use a lot of custom value types in dictionaries.
    /// </summary>
    public struct HashCalculator
    {
        /* Based on https://code.google.com/p/smhasher/source/browse/trunk/MurmurHash3.cpp
         * (which is public domain code by Austin Appleby), with a new convenient C# API.
         *
         * We use MurmurHash because it's proven to be fast and well-distributed, and also
         * because it has a really simple implementation.
         */

        public static readonly HashCalculator Value;

        private const uint c1 = 0xcc9e2d51;

        private const uint c2 = 0x1b873593;

        private uint _hash;

        static HashCalculator()
        {
            // Start with a different seed each time to make hash values more unpredictable.
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] seedBytes = new byte[4];
                rng.GetBytes(seedBytes);
                uint seedValue = BitConverter.ToUInt32(seedBytes, 0);
                Value = new HashCalculator(seedValue);
            }
        }

        private HashCalculator(uint hash)
        {
            _hash = hash;
        }

        public HashCalculator this[uint k]
        {
            get
            {
                k *= c1;
                k = RotL(k, 15);
                k *= c2;

                uint h = _hash ^ k;
                h = RotL(h, 13);
                h = h * 5 + 0xe6546b64;

                return new HashCalculator(h);
            }
        }

        public HashCalculator this[int k] => this[(uint)k];

        public HashCalculator this[long k] => this[(uint)k][(uint)(k >> 32)];

        public HashCalculator this[ulong k] => this[(uint)k][(uint)(k >> 32)];

        public HashCalculator this[float k] => this[k.GetHashCode()];

        public HashCalculator this[double k] => this[k.GetHashCode()];

        public HashCalculator this[bool k] => this[k ? 0xb0f0c498U : 0x859a53a4U]; // random values

        public HashCalculator this[byte[] bytes]
        {
            get
            {
                int l = bytes == null ? 0 : bytes.Length;
                int i;
                uint h = _hash;

                for (i = 0; i + 3 < l; i += 4)
                {
                    uint k = (uint)bytes[i] | ((uint)bytes[i + 1] << 8) | ((uint)bytes[i + 2] << 16) | ((uint)bytes[i + 3] << 24);
                    h = Mix(h, k);
                }

                if (i < l)
                {
                    uint k = 0;
                    for (int j = i; j < l; ++j)
                    {
                        k = (k << 8) | bytes[j];
                    }

                    h = Mix(h, k);
                }

                return new HashCalculator(h);
            }
        }

        public HashCalculator this[string s] => this[s.GetHashCode()];

        public static implicit operator int (HashCalculator murmurHash)
        {
            uint h = murmurHash._hash;

            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;

            return (int)h;
        }

        public override int GetHashCode()
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            return obj is HashCalculator && ((HashCalculator)obj)._hash == _hash;
        }

        private static uint Mix(uint h, uint k)
        {
            k *= c1;
            k = RotL(k, 15);
            k *= c2;

            h ^= k;
            h = RotL(h, 13);
            h = h * 5 + 0xe6546b64;

            return h;
        }

        private static uint RotL(uint v, int r)
        {
            return (v << r) | (v >> (32 - r));
        }
    }
}
