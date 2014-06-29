using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Data
{
    [EditorData]
    public class ModMetaData
    {
        [EditorData]
        public string Title { get; set; }

        [EditorData]
        public string Version { get; set; }

        [EditorData]
        public string Date { get; set; }

        [EditorData]
        public string Description { get; set; }
    }
}
