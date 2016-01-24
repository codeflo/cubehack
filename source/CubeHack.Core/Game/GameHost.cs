// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.Geometry;
using CubeHack.State;
using CubeHack.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using static CubeHack.Geometry.GeometryConstants;

namespace CubeHack.Game
{
    public sealed class GameHost : IDisposable
    {
        private readonly object _mutex = new object();

        private readonly HashSet<Channel> _channels = new HashSet<Channel>();

        private readonly List<BlockUpdateData> _blockUpdates = new List<BlockUpdateData>();

        private readonly WorldGenerator _worldGenerator = new WorldGenerator();
        private volatile bool _isDisposed;

        static GameHost()
        {
            ThreadPoolConfiguration.Init();
        }

        public GameHost(Universe universe, Mod mod)
        {
            Universe = universe;

            Mod = mod;

            ModData = new ModData
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
                MobType type = null;
                if (Mod.MobTypes.Count > 0)
                {
                    string nextMobType = Mod.MobTypes.Keys.ElementAt(i % Mod.MobTypes.Count);
                    type = Mod.MobTypes[nextMobType];
                }

                var e = new Entity(Universe);
                e.Set(Movement.Respawn(new PositionComponent()));
                e.Set(new AiComponent());
                e.Set(new MobTypeComponent(type?.Name));
            }

            var thread = new Thread(() => RunUniverse());
            thread.IsBackground = true;
            thread.Start();
        }

        public Universe Universe { get; }

        public Mod Mod { get; }

        public ModData ModData { get; }

        public void Dispose()
        {
            _isDisposed = true;
        }

        public IChannel ConnectPlayer()
        {
            Log.Info("Player connected");

            lock (_mutex)
            {
                var entity = new Entity(Universe);
                entity.Set(Movement.Respawn(new PositionComponent()));

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
                channel.Player.Destroy();
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
                    Universe.StartWorld[blockUpdate.Pos] = blockUpdate.Material;
                }
            }
        }

        private GameEvent GetCurrentGameEvent(Channel channel)
        {
            var player = channel.Player;

            var gameEvent = new GameEvent() { EntityInfos = new List<EntityData>() };

            foreach (var entity in Universe.GetEntitiesWithComponent<PositionComponent>())
            {
                if (entity != player)
                {
                    var position = entity.Get<PositionComponent>();
                    gameEvent.EntityInfos.Add(
                        new EntityData
                        {
                            PositionData = position,
                            ModelIndex = Mod.MobTypes.GetOrDefault(entity.Get<MobTypeComponent>()?.MobType)?.Model?.Index,
                        });
                }
            }

            PositionComponent playerPosition;
            if (player.TryGet(out playerPosition))
            {
                var playerChunkPos = (ChunkPos)playerPosition.Placement.Pos;

                int chunksInPacket = 0;

                ChunkPos.IterateOutwards(
                    playerChunkPos,
                    ChunkViewRadiusXZ,
                    ChunkViewRadiusY,
                    chunkPos =>
                    {
                        if (chunksInPacket < 5 && !channel.SentChunks.ContainsKey(chunkPos))
                        {
                            var chunk = Universe.StartWorld.GetChunk(chunkPos);

                            if (!chunk.IsCreated)
                            {
                                _worldGenerator.OnChunkRequested(chunk);
                            }
                            else
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
                        foreach (var entity in Universe.GetEntitiesWithComponent<PositionComponent>())
                        {
                            if (entity.Has<AiComponent>())
                            {
                                Ai.Control(Mod.PhysicsValues, elapsedDuration, entity);
                            }

                            var position = entity.Get<PositionComponent>();

                            Movement.MoveEntity(
                                Mod.PhysicsValues,
                                Universe.StartWorld,
                                position,
                                elapsedDuration,
                                position.Velocity);

                            entity.Set(position);
                        }

                        foreach (var channel in _channels)
                        {
                            if (channel.NeedsGameEvent())
                            {
                                var gameEvent = GetCurrentGameEvent(channel);
                                if (!channel.HasSentInitialValues)
                                {
                                    channel.HasSentInitialValues = true;
                                    gameEvent.PhysicsValues = Mod.PhysicsValues;
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
                channel.Player.Set(playerEvent.PositionData);
            }

            if (playerEvent.BlockUpdates != null)
            {
                AddBlockUpdates(playerEvent.BlockUpdates);
            }
        }
    }
}
