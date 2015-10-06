// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Game;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CubeHack.Tcp
{
    public class TcpServer
    {
        private readonly Universe _universe;

        public TcpServer(Universe universe)
        {
            _universe = universe;
            Task.Run(() => RunHost());
        }

        private async Task RunHost()
        {
            var listener = new TcpListener(IPAddress.Any, TcpConstants.Port);
            listener.Start();

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                var spawnedTask = Task.Run(() => RunConnection(client));
            }
        }

        private async Task RunConnection(TcpClient client)
        {
            IChannel internalChannel = null;

            try
            {
                client.NoDelay = true;

                var stream = client.GetStream();
                await ReadCookie(stream);

                await stream.WriteObjectAsync(_universe.ModData);

                using (internalChannel = _universe.ConnectPlayer())
                {
                    internalChannel.OnGameEventAsync += e => SendGameEventAsync(stream, e);

                    while (true)
                    {
                        var playerEvent = await stream.ReadObjectAsync<PlayerEvent>();
                        await internalChannel.SendPlayerEventAsync(playerEvent);
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                client.Close();
            }
        }

        private Task SendGameEventAsync(Stream stream, GameEvent e)
        {
            return stream.WriteObjectAsync(e);
        }

        private async Task ReadCookie(Stream stream)
        {
            byte[] cookieBytes = new byte[TcpConstants.MAGIC_COOKIE.Length];
            await stream.ReadArrayAsync(cookieBytes);
            for (int i = 0; i < TcpConstants.MAGIC_COOKIE.Length; ++i)
            {
                if (cookieBytes[i] != TcpConstants.MAGIC_COOKIE[i])
                {
                    throw new Exception("Client not recognized.");
                }
            }
        }
    }
}
