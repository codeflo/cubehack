// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Data;
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

        readonly World _startWorld = new World();

        readonly List<CubeUpdateData> _cubeUpdates = new List<CubeUpdateData>();

        public Universe(Mod mod)
        {
            _mod = mod;
            _startWorld.Generator = new WorldGenerator(_startWorld);

            _modData = new ModData
            {
                Materials = mod.Materials.Values.Select((m, i) =>
                {
                    m.Index = i;
                    return m;
                }).ToList(),
            };

            Task.Run(() => RunUniverse());
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

                if (player.PositionData != null)
                {
                    int chunkX = player.PositionData.Position.ChunkX;
                    int chunkY = player.PositionData.Position.ChunkY;
                    int chunkZ = player.PositionData.Position.ChunkZ;

                    for (int x = chunkX - 5; x <= chunkX + 5; ++x)
                    {
                        for (int y = chunkY - 5; y <= chunkY + 5; ++y)
                        {
                            for (int z = chunkZ - 5; z <= chunkZ + 5; ++z)
                            {
                                if (!channel.SentChunks[x, y, z])
                                {
                                    channel.SentChunks[x, y, z] = true;
                                    var chunk = _startWorld.GetChunk(x, y, z);

                                    if (chunk != null)
                                    {
                                        if (gameEvent.ChunkDataList == null)
                                        {
                                            gameEvent.ChunkDataList = new List<ChunkData>();
                                        }

                                        gameEvent.ChunkDataList.Add(chunk.GetChunkData());
                                    }
                                }
                            }
                        }
                    }
                }

                gameEvent.IsFrozen = player.PositionData == null;

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
    }
}
