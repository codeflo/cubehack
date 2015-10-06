// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using ProtoBuf;
using System;

namespace CubeHack.Data
{
    [ProtoContract]
    [EditorData]
    public class Color
    {
        public Color()
        {
        }

        public Color(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return;
            s = s.Trim();

            if (s.StartsWith("#"))
            {
                if (s.Length == 7)
                {
                    R = FromHex(s.Substring(1, 2)) / 255f;
                    G = FromHex(s.Substring(3, 2)) / 255f;
                    B = FromHex(s.Substring(5, 2)) / 255f;
                    return;
                }
                else if (s.Length == 4)
                {
                    R = FromHex(s.Substring(1, 1)) / 15f;
                    G = FromHex(s.Substring(2, 1)) / 15f;
                    B = FromHex(s.Substring(3, 1)) / 15f;
                    return;
                }
            }

            throw new ArgumentException();
        }

        [ProtoMember(1)]
        public float R { get; set; }

        [ProtoMember(2)]
        public float G { get; set; }

        [ProtoMember(3)]
        public float B { get; set; }

        private float FromHex(string s)
        {
            return (float)Convert.ToInt32(s, 16);
        }
    }
}
