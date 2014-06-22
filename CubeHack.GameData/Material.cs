// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.GameData
{
    [System.ComponentModel.TypeConverter(typeof(IncludeTypeConverter))]
    public class Material
    {
        public string Name { get; set; }

        public Texture Texture { get; set; }
    }
}
