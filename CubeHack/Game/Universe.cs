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

        readonly List<Entity> _entities = new List<Entity>();
        readonly HashSet<Channel> _channels = new HashSet<Channel>();

        public Universe(Mod mod)
        {
            _mod = mod;
            Task.Run(() => RunUniverse());
        }

        public Mod Mod
        {
            get
            {
                return _mod;
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
            var gameEvent = new GameEvent() { Entities = new List<GameEvent.EntityData>() };
            lock (_mutex)
            {
                foreach (var entity in _entities)
                {
                    if (entity != player)
                    {
                        lock (entity.Mutex)
                        {
                            gameEvent.Entities.Add(new GameEvent.EntityData { X = entity.X, Y = entity.Y, Z = entity.Z });
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
    }
}
