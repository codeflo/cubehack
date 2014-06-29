// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    internal sealed class Channel : IChannel
    {
        readonly Universe _universe;
        readonly Entity _player;

        public Channel(Universe universe, Entity player)
        {
            _universe = universe;
            _player = player;
            SentChunks = new Dictionary3D<bool>();
        }

        public int SentCubeUpdates { get; set; }

        public Dictionary3D<bool> SentChunks { get; private set; }

        Func<GameEvent, Task> _onGameEventAsync;
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

        private ConcurrentQueue<PlayerEvent> _playerEvents = new ConcurrentQueue<PlayerEvent>();
        public PlayerEvent TakePlayerEvent()
        {
            PlayerEvent ret;
            _playerEvents.TryDequeue(out ret);
            return ret;
        }

        private ConcurrentQueue<GameEvent> _gameEvents = new ConcurrentQueue<GameEvent>();

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
