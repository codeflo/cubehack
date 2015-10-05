// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

namespace CubeHack.Game
{
    public class Dictionary3D<T>
    {
        private int _bucketCount = 2048;
        private Entry[] _buckets = new Entry[2048];

        public T this[int x, int y, int z]
        {
            get
            {
                int hashCode = GetHashCode(x, y, z);
                int bucket = hashCode & (_bucketCount - 1);

                Entry e = _buckets[bucket];
                while (e != null)
                {
                    if (e.X == x && e.Y == y && e.Z == z)
                    {
                        return e.Value;
                    }

                    e = e.Next;
                }

                return default(T);
            }

            set
            {
                int hashCode = GetHashCode(x, y, z);
                int bucket = hashCode & (_bucketCount - 1);

                Entry firstBucket = _buckets[bucket];
                Entry e = firstBucket;
                while (e != null)
                {
                    if (e.X == x && e.Y == y && e.Z == z)
                    {
                        e.Value = value;
                    }

                    e = e.Next;
                }

                _buckets[bucket] = new Entry
                {
                    X = x,
                    Y = y,
                    Z = z,
                    Value = value,
                    Next = firstBucket,
                };
            }
        }

        private static int GetHashCode(int x, int y, int z)
        {
            return (x * 777 + y) * 111111 + z;
        }

        private class Entry
        {
            public int X, Y, Z;

            public T Value;

            public Entry Next;
        }
    }
}
