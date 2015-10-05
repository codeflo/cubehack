// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Game;
using CubeHack.Tcp;
using System.Threading;

namespace CubeHack.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var universe = new Universe(DataLoader.LoadMod("Core"));
            new TcpServer(universe);

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
