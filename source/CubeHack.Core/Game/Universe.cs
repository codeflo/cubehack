// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.Geometry;
using CubeHack.Storage;
using CubeHack.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using static CubeHack.Geometry.GeometryConstants;

namespace CubeHack.Game
{
    public sealed class Universe : IDisposable
    {
        private readonly object _mutex = new object();

        private readonly Mod _mod;
        private readonly ModData _modData;

        private readonly List<Entity> _entities = new List<Entity>();
        private readonly HashSet<Channel> _channels = new HashSet<Channel>();

        private readonly World _startWorld;

        private readonly List<BlockUpdateData> _blockUpdates = new List<BlockUpdateData>();

        private volatile bool _isDisposed;

        static Universe()
        {
            ThreadPoolConfiguration.Init();
        }

        public Universe(ISaveFile saveFile, Mod mod)
        {
            SaveFile = saveFile;
            _mod = mod;

            _startWorld = new World(this);
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

        public ISaveFile SaveFile { get; }

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
            if (!_isDisposed)
            {
                _isDisposed = true;
                SaveFile.Dispose();
            }
        }

        public IChannel ConnectPlayer()
        {
            Log.Info("Player connected");

            lock (_mutex)
            {
                var entity = new Entity();
                _entities.Add(entity);

                entity.PositionData = new PositionData();
                Movement.Respawn(entity.PositionData);

                var channel = new Channel(this, entity);
                _channels.Add(channel);

                channel.SentBlockUpdates = _blockUpdates.Count;
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

        private void AddBlockUpdates(List<BlockUpdateData> blockUpdates)
        {
            lock (_mutex)
            {
                _blockUpdates.AddRange(blockUpdates);

                foreach (var blockUpdate in blockUpdates)
                {
                    _startWorld[blockUpdate.Pos] = blockUpdate.Material;
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
                var playerChunkPos = (ChunkPos)player.PositionData.Placement.Pos;

                int chunksInPacket = 0;

                ChunkPos.IterateOutwards(
                    playerChunkPos,
                    ChunkViewRadiusXZ,
                    ChunkViewRadiusY,
                    chunkPos =>
                    {
                        if (!channel.SentChunks.ContainsKey(chunkPos))
                        {
                            var chunk = _startWorld.GetChunk(chunkPos);

                            if (chunksInPacket < 5 && chunk.IsCreated)
                            {
                                channel.SentChunks[chunkPos] = true;

                                if (chunk != null)
                                {
                                    if (gameEvent.ChunkDataList == null)
                                    {
                                        gameEvent.ChunkDataList = new List<ChunkData>();
                                    }

                                    gameEvent.ChunkDataList.Add(chunk.GetChunkData());
                                    ++chunksInPacket;
                                }
                            }
                        }
                    });
            }

            if (channel.SentBlockUpdates != _blockUpdates.Count)
            {
                gameEvent.BlockUpdates = _blockUpdates.GetRange(channel.SentBlockUpdates, _blockUpdates.Count - channel.SentBlockUpdates);
                channel.SentBlockUpdates = _blockUpdates.Count;
            }

            return gameEvent;
        }

        private void RunUniverse()
        {
            try
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
                                Ai.Control(_mod.PhysicsValues, elapsedDuration, entity, _entities.FindAll(e => !e.Equals(entity)));
                            }

                            Movement.MoveEntity(
                                _mod.PhysicsValues,
                                _startWorld,
                                entity.PositionData,
                                elapsedDuration,
                                entity.PositionData.Velocity);
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
            catch
            {
            }
        }

        private void HandlePlayerEvent(Channel channel, PlayerEvent playerEvent)
        {
            if (playerEvent.PositionData != null)
            {
                channel.Player.PositionData = playerEvent.PositionData;
            }

            if (playerEvent.BlockUpdates != null)
            {
                AddBlockUpdates(playerEvent.BlockUpdates);
            }
        }
    }
}
