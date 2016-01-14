// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System.Globalization;
using System.Linq;
using System.Text;

namespace CubeHack.Storage
{
    /// <summary>
    /// Helper class to convert bytes into their hexadecimal representation.
    /// </summary>
    public static class HexConverter
    {
        /* Adapted from http://stackoverflow.com/a/18574846 */

        public static string[] _hexBytes = Enumerable.Range(0, 256).Select(v => v.ToString("x2", CultureInfo.InvariantCulture)).ToArray();

        public static string ToHex(byte[] buffer)
        {
            return ToHex(buffer, 0, buffer.Length);
        }

        public static string ToHex(byte[] buffer, int offset, int length)
        {
            StringBuilder s = new StringBuilder(length * 2);
            for (int i = offset; i < offset + length; ++i)
            {
                s.Append(_hexBytes[buffer[i]]);
            }

            return s.ToString();
        }
    }
}
