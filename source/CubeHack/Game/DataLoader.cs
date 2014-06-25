// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Data;
using Newtonsoft.Json.Linq;
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
    static class DataLoader
    {
        static readonly string modDir = Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "Mods");

        public static Mod LoadMod(string name)
        {
            var modItem = CubeHack.DataModel.Item.Create(typeof(Mod));
            string s = File.ReadAllText(Path.Combine(modDir, name + ".cubemod.json"), Encoding.UTF8);
            var json = JToken.Parse(s);
            modItem.Load(json);
            return (Mod)modItem.GetObject();
        }
    }
}
