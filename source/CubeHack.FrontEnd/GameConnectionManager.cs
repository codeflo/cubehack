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
        private readonly GameController _controller;

        private readonly WorldTextureAtlas _textureAtlas;
        private IDisposable _server;

        [DependencyInjected]
        internal GameConnectionManager(GameController controller, WorldTextureAtlas textureAtlas)
        {
            _controller = controller;
            _textureAtlas = textureAtlas;
        }

        public GameClient Client { get; private set; }

        public bool IsConnected => Client != null && Client.IsConnected;

        public bool IsConnecting { get; private set; }

        public void Disconnect()
        {
            if (Client != null)
            {
                var client = Client;
                Task.Run(() => client.Dispose());
                Client = null;
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
            await _textureAtlas.SetModAsync(channel.ModData);
            Client = new GameClient(_controller, channel);
        }

        private async Task<TcpChannel> ConnectTcpAsync(string address, int port)
        {
            var channel = new TcpChannel(address, port);
            await channel.ConnectAsync();
            return channel;
        }
    }
}
