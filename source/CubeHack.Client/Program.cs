// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Game;
using CubeHack.Tcp;

namespace CubeHack.Client
{
    internal static class Program
    {
        public static void Main()
        {
            string host = Microsoft.VisualBasic.Interaction.InputBox("Enter server address:\n\n(Leave blank for single player.)", "CubeHack", " ");
            if (host == null || host.Length == 0) return;

            int port = 0;
            TcpServer server = null;
            if (string.IsNullOrWhiteSpace(host))
            {
                server = new TcpServer(new Universe(DataLoader.LoadMod("Core")), false);
                port = server.Port;
            }

            using (server)
            {
                var gameApp = new GameApp();
                gameApp.Connect(host, port);
                gameApp.Run();
            }
        }
    }
}
