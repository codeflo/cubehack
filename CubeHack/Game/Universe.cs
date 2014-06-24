// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    sealed class Universe : IDisposable
    {
        readonly object _mutex = new object();

        readonly Mod _mod;
        readonly ModData _modData;

        readonly List<Entity> _entities = new List<Entity>();
        readonly HashSet<Channel> _channels = new HashSet<Channel>();

        readonly ChunkData _exampleChunkData;

        readonly List<CubeUpdateData> _cubeUpdates = new List<CubeUpdateData>();

        public Universe(Mod mod)
        {
            _mod = mod;
            _exampleChunkData = CreateExampleChunkData();
            _modData = new ModData
            {
                Textures = new List<Texture> { _mod.DefaultMaterial.Texture },
            };

            Task.Run(() => RunUniverse());
        }

        public ChunkData ExampleChunkData
        {
            get
            {
                return _exampleChunkData;
            }
        }

        public Mod Mod
        {
            get
            {
                return _mod;
            }
        }

        public ModData ModData
        {
            get
            {
                return _modData;
            }
        }

        public void Dispose()
        {
        }

        public IChannel ConnectPlayer()
        {
            lock (_mutex)
            {
                var entity = new Entity();
                _entities.Add(entity);

                var channel = new Channel(this, entity);
                _channels.Add(channel);

                return channel;
            }
        }

        public void AddCubeUpdates(List<CubeUpdateData> cubeUpdates)
        {
            lock (_mutex)
            {
                _cubeUpdates.AddRange(cubeUpdates);
            }
        }

        public GameEvent GetCurrentGameEvent(Channel channel)
        {
            var player = channel.Player;

            var gameEvent = new GameEvent() { EntityPositions = new List<PositionData>() };
            lock (_mutex)
            {
                foreach (var entity in _entities)
                {
                    if (entity != player)
                    {
                        lock (entity.Mutex)
                        {
                            gameEvent.EntityPositions.Add(entity.PositionData);
                        }
                    }
                }

                if (channel.SentCubeUpdates != _cubeUpdates.Count)
                {
                    gameEvent.CubeUpdates = _cubeUpdates.GetRange(channel.SentCubeUpdates, _cubeUpdates.Count - channel.SentCubeUpdates);
                    channel.SentCubeUpdates = _cubeUpdates.Count;
                }
            }

            return gameEvent;
        }

        public void DeregisterChannel(Channel channel)
        {
            lock (_mutex)
            {
                _entities.Remove(channel.Player);
                _channels.Remove(channel);
            }
        }

        private async Task RunUniverse()
        {
            while (true)
            {
                await Task.Delay(100);
            }
        }

        private ChunkData CreateExampleChunkData()
        {
            var ret = new ChunkData();
            ret.X0 = -20;
            ret.X1 = 20;
            ret.Y0 = -20;
            ret.Y1 = 20;
            ret.Z0 = -20;
            ret.Z1 = 20;

            for (int x = ret.X0; x < ret.X1; ++x)
            {
                for (int y = ret.Y0; y <= -1; ++y)
                {
                    for (int z = ret.Z0; z < ret.Z1; ++z)
                    {
                        ret[x, y, z] = 1;
                    }
                }
            }

            for (int x = -3; x <= -2; ++x)
            {
                for (int y = 0; y <= 1; ++y)
                {
                    for (int z = -7; z <= -6; ++z)
                    {
                        ret[x, y, z] = 1;
                    }
                }
            }

            return ret;
        }
    }
}
