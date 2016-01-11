// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;
using System.Text;

namespace CubeHack.Game
{
    public static class DataLoader
    {
        private static readonly string _modDir = Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "Mods");

        public static Mod LoadMod(string name)
        {
            var modItem = CubeHack.EditorModel.Item.Create(typeof(Mod));
            string s = File.ReadAllText(Path.Combine(_modDir, name + ".cubemod.json"), Encoding.UTF8);
            var json = JToken.Parse(s);
            modItem.Load(json);
            return (Mod)modItem.GetObject();
        }
    }
}
