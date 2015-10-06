// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Game;
using CubeHack.Tcp;
using System;

namespace CubeHack.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var universe = new Universe(DataLoader.LoadMod("Core"));
            new TcpServer(universe);

            Console.WriteLine("Server running, press Q to quit.");
            while (Console.ReadKey(true).Key != ConsoleKey.Q) { }
        }
    }
}
