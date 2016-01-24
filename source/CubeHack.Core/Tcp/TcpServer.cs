// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Game;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CubeHack.Tcp
{
    public sealed class TcpServer : IDisposable
    {
        private readonly GameHost _universe;
        private TcpListener _listener;
        private int _port;

        public TcpServer(GameHost universe, bool runPublic)
        {
            _universe = universe;
            RunHost(runPublic);
        }

        public int Port => _port;

        public void Dispose()
        {
            try
            {
                _listener.Stop();
            }
            catch
            {
            }

            _universe.Dispose();
        }

        private async void RunHost(bool runPublic)
        {
            try
            {
                if (runPublic)
                {
                    _port = TcpConstants.Port;
                    _listener = TcpListener.Create(_port);
                    _listener.Start();
                }
                else
                {
                    _listener = new TcpListener(IPAddress.Loopback, 0);
                    _listener.Start();
                    _port = ((IPEndPoint)_listener.LocalEndpoint).Port;
                }

                while (true)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    var spawnedTask = Task.Run(() => RunConnection(client));
                }
            }
            catch
            {
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
            byte[] cookieBytes = new byte[TcpConstants.MagicCookie.Length];
            await stream.ReadArrayAsync(cookieBytes);
            for (int i = 0; i < TcpConstants.MagicCookie.Length; ++i)
            {
                if (cookieBytes[i] != TcpConstants.MagicCookie[i])
                {
                    throw new Exception("Client not recognized.");
                }
            }
        }
    }
}
