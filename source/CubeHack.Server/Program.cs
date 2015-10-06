// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Game;
using CubeHack.Tcp;
using System;

namespace CubeHack.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (new TcpServer(new Universe(DataLoader.LoadMod("Core")), true))
            {
                Console.WriteLine("Server running, press Q to quit.");
                while (Console.ReadKey(true).Key != ConsoleKey.Q) { }
            }
        }
    }
}
