// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.FrontEnd.Graphics.Rendering;
using CubeHack.Game;
using CubeHack.Storage;
using CubeHack.Tcp;
using System;
using System.Threading.Tasks;

namespace CubeHack.FrontEnd
{
    public sealed class GameConnectionManager : IDisposable
    {
        private readonly GameLoop _gameLoop;
        private readonly GameController _controller;
        private readonly WorldTextureAtlas _textureAtlas;

        private IChannel _channel;
        private IDisposable _server;

        [DependencyInjected]
        internal GameConnectionManager(GameLoop gameLoop, GameController controller, WorldTextureAtlas textureAtlas)
        {
            _gameLoop = gameLoop;
            _controller = controller;
            _textureAtlas = textureAtlas;
        }

        public GameClient Client { get; private set; }

        public bool IsConnected => Client != null;

        public bool IsConnecting { get; private set; }

        public void Disconnect()
        {
            if (Client != null)
            {
                Client = null;
            }

            if (_channel != null)
            {
                var channel = _channel;
                channel.OnGameEventAsync = null;
                Task.Run(() => channel.Dispose());
                _channel = null;
            }

            if (_server != null)
            {
                var server = _server;
                Task.Run(() => server.Dispose());
                _server = null;
            }
        }

        public async Task ConnectAsync(string address)
        {
            Disconnect();

            IsConnecting = true;
            try
            {
                var channel = await ConnectTcpAsync(address, 0);
                await ConnectChannelAsync(channel);
            }
            catch
            {
            }
            finally
            {
                IsConnecting = false;
            }
        }

        public async Task LoadGameAsync(string name)
        {
            Disconnect();

            IsConnecting = true;
            try
            {
                var saveFile = await SaveDirectory.OpenFileAsync(SaveDirectory.DebugGame);
                var server = await Task.Run(() => new TcpServer(new Universe(saveFile, DataLoader.LoadMod("Core")), true));
                _server = server;

                var channel = await ConnectTcpAsync("127.0.0.1", server.Port);
                await ConnectChannelAsync(channel);
            }
            catch
            {
            }
            finally
            {
                IsConnecting = false;
            }
        }

        public async Task ConnectUniverseAsync(Universe universe)
        {
            Disconnect();
            IsConnecting = true;
            try
            {
                var server = await Task.Run(() => new TcpServer(universe, true));
                _server = server;

                var channel = await ConnectTcpAsync("127.0.0.1", server.Port);
                await ConnectChannelAsync(channel);
            }
            catch
            {
            }
            finally
            {
                IsConnecting = false;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        private async Task ConnectChannelAsync(IChannel channel)
        {
            _channel = channel;
            channel.OnGameEventAsync = OnGameEventAsync;
            await _textureAtlas.SetModAsync(channel.ModData);
            Client = new GameClient(_controller, channel.ModData);
            await channel.SendPlayerEventAsync(Client.CreatePlayerEvent());
        }

        private async Task OnGameEventAsync(GameEvent gameEvent)
        {
            if (gameEvent == null) return;

            PlayerEvent playerEvent = null;
            await _gameLoop.SendAsync(() => playerEvent = OnGameEvent(gameEvent));
            if (playerEvent != null && _channel != null) await _channel.SendPlayerEventAsync(playerEvent);
        }

        private PlayerEvent OnGameEvent(GameEvent gameEvent)
        {
            if (gameEvent.IsDisconnected)
            {
                Disconnect();
                return null;
            }

            if (Client == null) return null;

            Client.HandleGameEvent(gameEvent);
            return Client.CreatePlayerEvent();
        }

        private async Task<TcpChannel> ConnectTcpAsync(string address, int port)
        {
            var channel = new TcpChannel(address, port);
            await channel.ConnectAsync();
            return channel;
        }
    }
}
