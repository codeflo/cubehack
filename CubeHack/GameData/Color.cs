// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.GameData
{
    [ProtoContract]
    [TypeConverter(typeof(ColorTypeConverter))]
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

        float FromHex(string s)
        {
            return (float)Convert.ToInt32(s, 16);
        }
    }
}
