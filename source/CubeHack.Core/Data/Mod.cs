using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Data
{
    [EditorData]
    public class Mod
    {
        [EditorData]
        public ModMetaData MetaData { get; set; }

        [EditorData]
        public PhysicsValues PhysicsValues { get; set; }

        [EditorData]
        public string DefaultMaterial { get; set; }

        [EditorData]
        public Dictionary<string, Material> Materials { get; set; }

        [EditorData]
        public Dictionary<string, MobType> MobTypes { get; set; }
    }
}
