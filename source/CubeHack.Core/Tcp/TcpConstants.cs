// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

namespace CubeHack.Tcp
{
    public class TcpConstants
    {
        public const int Port = 26031;

        public static readonly byte[] MAGIC_COOKIE = new byte[] {
            (byte)'C', (byte)'u', (byte)'b', (byte)'e', (byte)'H', (byte)'a', (byte)'c', (byte)'k',
            0, 0, 0, 6, /* increment these to change the protocol version */
        };
    }
}
