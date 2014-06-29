using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Data
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class EditorDataAttribute : Attribute
    {
    }
}
