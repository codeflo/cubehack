﻿// Copyright (c) 2014 the CubeHack authors. All rights reserved.
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

        public GameEvent GetCurrentGameEvent(Entity player)
        {
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
            ret.X0 = -10;
            ret.X1 = 10;
            ret.Y0 = -10;
            ret.Y1 = 10;
            ret.Z0 = -10;
            ret.Z1 = 10;

            for (int x = -10; x < 10; ++x)
            {
                for (int z = -10; z < 10; ++z)
                {
                    ret[x, -1, z] = 1;
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
