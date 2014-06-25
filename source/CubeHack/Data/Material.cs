﻿// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Data
{
    [EditorData]
    public class Material
    {
        [EditorData]
        public string Name { get; set; }

        [EditorData]
        public Texture Texture { get; set; }
    }
}
