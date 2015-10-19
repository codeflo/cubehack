// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CubeHack.Game
{
    public sealed class Universe : IDisposable
    {
        private readonly object _mutex = new object();

        private readonly Mod _mod;
        private readonly ModData _modData;

        private readonly List<Entity> _entities = new List<Entity>();
        private readonly HashSet<Channel> _channels = new HashSet<Channel>();

        private readonly World _startWorld = new World();

        private readonly List<CubeUpdateData> _cubeUpdates = new List<CubeUpdateData>();

        private volatile bool _isDisposed;

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

                Models = mod.MobTypes.Values.Select((m, i) =>
                {
                    m.Model.Index = i;
                    return m.Model;
                }).ToList(),
            };

            for (int i = 0; i < 20; ++i)
            {
                var e = new Entity()
                {
                    PositionData = new PositionData(),
                    IsAiControlled = true,
                };

                Movement.Respawn(e.PositionData);
                _entities.Add(e);
            }

            var thread = new Thread(() => RunUniverse());
            thread.IsBackground = true;
            thread.Start();
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
            _isDisposed = true;
        }

        public IChannel ConnectPlayer()
        {
            Log.Info("Player connected");

            lock (_mutex)
            {
                var entity = new Entity();
                _entities.Add(entity);

                var channel = new Channel(this, entity);
                _channels.Add(channel);

                channel.SentCubeUpdates = _cubeUpdates.Count;
                return channel;
            }
        }

        internal void DeregisterChannel(Channel channel)
        {
            Log.Info("Player disconnected");

            lock (_mutex)
            {
                _entities.Remove(channel.Player);
                _channels.Remove(channel);
            }
        }

        private void AddCubeUpdates(List<CubeUpdateData> cubeUpdates)
        {
            lock (_mutex)
            {
                _cubeUpdates.AddRange(cubeUpdates);

                foreach (var cubeUpdate in cubeUpdates)
                {
                    _startWorld[cubeUpdate.X, cubeUpdate.Y, cubeUpdate.Z] = cubeUpdate.Material;
                }
            }
        }

        private GameEvent GetCurrentGameEvent(Channel channel)
        {
            var player = channel.Player;

            var gameEvent = new GameEvent() { EntityPositions = new List<PositionData>() };
            foreach (var entity in _entities)
            {
                if (entity != player)
                {
                    gameEvent.EntityPositions.Add(entity.PositionData);
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

            return gameEvent;
        }

        private void RunUniverse()
        {
            var frameTime = GameTime.Now();

            while (!_isDisposed)
            {
                var startTime = DateTime.Now;

                lock (_mutex)
                {
                    foreach (var channel in _channels)
                    {
                        while (true)
                        {
                            var playerEvent = channel.TakePlayerEvent();
                            if (playerEvent == null) break;
                            HandlePlayerEvent(channel, playerEvent);
                        }
                    }

                    var elapsedDuration = GameTime.Update(ref frameTime);
                    foreach (var entity in _entities)
                    {
                        if (entity.PositionData == null)
                        {
                            continue;
                        }

                        if (entity.IsAiControlled)
                        {
                            Ai.Control(_mod.PhysicsValues, elapsedDuration, entity);
                        }

                        Movement.MoveEntity(
                            _mod.PhysicsValues,
                            _startWorld,
                            entity.PositionData,
                            elapsedDuration,
                            entity.PositionData.Velocity.X,
                            entity.PositionData.Velocity.Y,
                            entity.PositionData.Velocity.Z);
                    }

                    foreach (var channel in _channels)
                    {
                        if (channel.NeedsGameEvent())
                        {
                            var gameEvent = GetCurrentGameEvent(channel);
                            if (!channel.HasSentInitialValues)
                            {
                                channel.HasSentInitialValues = true;
                                gameEvent.PhysicsValues = _mod.PhysicsValues;
                            }

                            channel.QueueGameEvent(gameEvent);
                        }
                    }
                }

                var endTime = DateTime.Now;

                double sleepMillis = 10 - (endTime - startTime).TotalMilliseconds;
                if (sleepMillis > 0)
                {
                    Thread.Sleep((int)Math.Ceiling(sleepMillis));
                }
            }
        }

        private void HandlePlayerEvent(Channel channel, PlayerEvent playerEvent)
        {
            if (playerEvent.PositionData != null)
            {
                channel.Player.PositionData = playerEvent.PositionData;
            }

            if (playerEvent.CubeUpdates != null)
            {
                AddCubeUpdates(playerEvent.CubeUpdates);
            }
        }
    }
}
