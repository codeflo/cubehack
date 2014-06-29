using CubeHack.Game;
using CubeHack.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CubeHack.Server
{
    class Program
    {
        static void Main(string[] args)
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
