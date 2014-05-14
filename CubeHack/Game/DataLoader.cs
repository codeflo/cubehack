// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xaml;

namespace CubeHack.Game
{
    class DataLoader
    {
        static readonly string modDir = Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "mods");

        static readonly ThreadLocal<string> _dir = new ThreadLocal<string>();

        public static Mod LoadMod(string name)
        {
            // TODOs:
            // * sanitize the name (no ../../foo for example)
            // * handle IO errors
            // * handle XAML errors
            // * whitelist the XAML namespace
            
            _dir.Value = Path.Combine(modDir, name);
            try
            {
                return (Mod)Resolve("_mod.xaml");
            }
            finally
            {
                _dir.Value = null;
            }
        }

        public static object Resolve(string path)
        {
            string oldDir = _dir.Value;
            string file = Path.Combine(_dir.Value, path);
            _dir.Value = Path.GetDirectoryName(file);

            try
            {
                return XamlServices.Load(file);
            }
            finally
            {
                _dir.Value = oldDir;
            }
        }
    }
}
