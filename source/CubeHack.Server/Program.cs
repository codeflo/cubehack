// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Game;
using CubeHack.State;
using CubeHack.Storage;
using CubeHack.Tcp;
using System;
using System.Threading.Tasks;

namespace CubeHack.Server
{
    internal class Program
    {
        private static void Main()
        {
            Task.Run(() => MainAsync()).Wait();
        }

        private static async Task MainAsync()
        {
            var saveFile = await SaveDirectory.OpenFileAsync(SaveDirectory.DebugGame);
            using (new TcpServer(new GameHost(new Universe(saveFile), DataLoader.LoadMod("Core")), true))
            {
                Console.WriteLine("Server running, press Q to quit.");
                while (Console.ReadKey(true).Key != ConsoleKey.Q) { }
            }
        }
    }
}
