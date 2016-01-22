// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System.Collections.Generic;
using System.IO;

namespace CubeHack.Storage
{
    public static class Serialization
    {
        public static byte[] Serialize<T>(T instance)
        {
            using (var memoryStream = new MemoryStream())
            {
                if (!EqualityComparer<T>.Default.Equals(instance, default(T)))
                {
                    ProtoBuf.Serializer.Serialize(memoryStream, instance);
                }

                return memoryStream.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return default(T);

            using (var memoryStream = new MemoryStream(bytes))
            {
                return ProtoBuf.Serializer.Deserialize<T>(memoryStream);
            }
        }
    }
}
