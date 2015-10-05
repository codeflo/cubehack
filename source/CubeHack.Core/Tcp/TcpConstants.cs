// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

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
