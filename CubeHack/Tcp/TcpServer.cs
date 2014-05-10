// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Game;
using CubeHack.Tcp;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Tcp
{
    class TcpServer
    {
        readonly Universe _universe;

        public TcpServer(Universe universe)
        {
            _universe = universe;
            Task.Run(() => RunHost());
        }

        async Task RunHost()
        {
            var listener = new TcpListener(IPAddress.Any, TcpConstants.Port);
            listener.Start();

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                Task.Run(() => RunConnection(client)).Forget();
            }
        }

        async Task RunConnection(TcpClient client)
        {
            try
            {
                client.NoDelay = true;

                var stream = client.GetStream();
                await ReadCookie(stream);

                var internalChannel = _universe.ConnectPlayer();
                internalChannel.OnGameEventAsync += e => SendGameEventAsync(stream, e);

                while (true)
                {
                    var playerEvent = await stream.ReadObjectAsync<PlayerEvent>();
                    internalChannel.SendPlayerEvent(playerEvent);
                }
            }
            catch (Exception)
            {
                // TODO: log
            }
            finally
            {
                client.Close();
            }
        }

        Task SendGameEventAsync(Stream stream, GameEvent e)
        {
            return stream.WriteObjectAsync(e);
        }

        async Task ReadCookie(Stream stream)
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
