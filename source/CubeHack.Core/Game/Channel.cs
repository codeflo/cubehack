// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Geometry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    internal sealed class Channel : IChannel
    {
        private readonly Universe _universe;
        private readonly Entity _player;

        private Func<GameEvent, Task> _onGameEventAsync;

        private ConcurrentQueue<PlayerEvent> _playerEvents = new ConcurrentQueue<PlayerEvent>();

        private ConcurrentQueue<GameEvent> _gameEvents = new ConcurrentQueue<GameEvent>();

        public Channel(Universe universe, Entity player)
        {
            _universe = universe;
            _player = player;
        }

        public int SentCubeUpdates { get; set; }

        public Dictionary<ChunkPos, bool> SentChunks { get; } = new Dictionary<ChunkPos, bool>();

        public Func<GameEvent, Task> OnGameEventAsync
        {
            get
            {
                return _onGameEventAsync;
            }

            set
            {
                _onGameEventAsync = value;
            }
        }

        public Entity Player
        {
            get { return _player; }
        }

        public ModData ModData
        {
            get
            {
                return _universe.ModData;
            }
        }

        public bool HasSentInitialValues
        {
            get;
            set;
        }

        public PlayerEvent TakePlayerEvent()
        {
            PlayerEvent ret;
            _playerEvents.TryDequeue(out ret);
            return ret;
        }

        public async Task SendPlayerEventAsync(PlayerEvent playerEvent)
        {
            Func<GameEvent, Task> onGameEventAsync;

            _playerEvents.Enqueue(playerEvent);

            onGameEventAsync = OnGameEventAsync;

            if (onGameEventAsync != null)
            {
                if (_gameEvents.Count == 0)
                {
                    // Always send an empty GameEvent as an ACK.
                    await onGameEventAsync(new GameEvent());
                }
                else
                {
                    while (true)
                    {
                        GameEvent gameEvent;
                        _gameEvents.TryDequeue(out gameEvent);
                        if (gameEvent == null) break;
                        await onGameEventAsync(gameEvent);
                    }
                }
            }
        }

        public bool NeedsGameEvent()
        {
            return _gameEvents.Count == 0;
        }

        public void QueueGameEvent(GameEvent gameEvent)
        {
            _gameEvents.Enqueue(gameEvent);
        }

        public void Dispose()
        {
            _universe.DeregisterChannel(this);
        }
    }
}
