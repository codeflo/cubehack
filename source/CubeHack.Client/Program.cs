// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Client
{
    public static class Program
    {
        public static void Main()
        {
            var host = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();
            new MainWindow().Run(host);
        }
    }
}
