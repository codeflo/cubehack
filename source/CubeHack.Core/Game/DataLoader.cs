// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Data;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;
using System.Text;

namespace CubeHack.Game
{
    public static class DataLoader
    {
        private static readonly string modDir = Path.Combine(
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
