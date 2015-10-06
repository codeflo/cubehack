// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System.Collections.Generic;

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
