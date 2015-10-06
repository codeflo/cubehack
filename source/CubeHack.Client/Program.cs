// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Linq;

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
