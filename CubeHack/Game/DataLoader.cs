// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace CubeHack.Game
{
    class DataLoader
    {
        static readonly string modDir = Path.Combine(
            Path.GetDirectoryName( Assembly.GetEntryAssembly().Location ),
            "mods");

        public static Mod LoadMod(string name)
        {
            // TODOs:
            //   * sanitize the name (no ../../foo for example)
            //   * handle IO errors
            //   * handle XAML errors
            //   * whitelist the XAML namespace
            var mod = (Mod)XamlServices.Load(Path.Combine(modDir, name, "_mod.xaml"));
            return mod;
        }
    }
}
