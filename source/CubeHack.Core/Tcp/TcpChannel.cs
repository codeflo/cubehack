﻿// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Game;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CubeHack.Tcp
{
    public sealed class TcpChannel : IChannel
    {
        private readonly object _mutex = new object();
        private readonly TcpClient _tcpClient;
        private readonly string _host;
        private readonly int _port;

        private Stream _stream;
        private bool _isConnected;

        private Func<GameEvent, Task> _onGameEventAsync;

        public TcpChannel(string host, int port)
        {
            _host = host;
            _port = port == 0 ? TcpConstants.Port : port;
            _tcpClient = new TcpClient();
        }

        public ModData ModData { get; private set; }

        public Func<GameEvent, Task> OnGameEventAsync
        {
            get
            {
                lock (_mutex)
                {
                    return _onGameEventAsync;
                }
            }

            set
            {
                lock (_mutex)
                {
                    _onGameEventAsync = value;
                }
            }
        }

        public async Task ConnectAsync()
        {
            _tcpClient.NoDelay = true;
            await _tcpClient.ConnectAsync(_host, _port);

            _stream = _tcpClient.GetStream();
            await _stream.WriteAsync(TcpConstants.MagicCookie, 0, TcpConstants.MagicCookie.Length);
            await _stream.FlushAsync();

            ModData = await _stream.ReadObjectAsync<ModData>();

            _isConnected = true;
            var spawnedTask = Task.Run(() => RunChannel());
        }

        public async Task SendPlayerEventAsync(PlayerEvent playerEvent)
        {
            if (!_isConnected)
            {
                return;
            }

            await _stream.WriteObjectAsync(playerEvent);
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }

        private async Task RunChannel()
        {
            try
            {
                while (true)
                {
                    var gameEvent = await _stream.ReadObjectAsync<GameEvent>();

                    Func<GameEvent, Task> onGameEventAsync;
                    lock (_mutex)
                    {
                        onGameEventAsync = _onGameEventAsync;
                    }

                    if (onGameEventAsync != null) await onGameEventAsync(gameEvent);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                Func<GameEvent, Task> onGameEventAsync;
                lock (_mutex)
                {
                    onGameEventAsync = _onGameEventAsync;
                }

                if (onGameEventAsync != null) await onGameEventAsync(new GameEvent { IsDisconnected = true });
            }
        }
    }
}
