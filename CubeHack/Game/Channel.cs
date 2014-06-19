// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    sealed class Channel : IChannel
    {
        readonly object _mutex = new object();
        readonly Universe _universe;
        readonly Entity _player;

        bool hasSentInitialValues = false;

        public Channel(Universe universe, Entity player)
        {
            _universe = universe;
            _player = player;

            Task.Run(() => RunChannel());
        }

        Func<GameEvent, Task> _onGameEventAsync;
        public Func<GameEvent, Task> OnGameEventAsync
        {
            get
            {
                lock (_mutex)
                {
                    return _onGameEventAsync;
                }
            }

            set
            {
                lock (_mutex)
                {
                    _onGameEventAsync = value;
                }
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

        public void SendPlayerEvent(PlayerEvent playerEvent)
        {
            lock (_player.Mutex)
            {
                _player.PositionData = playerEvent.PositionData;
            }
        }

        private async Task RunChannel()
        {
            try
            {
                while (true)
                {
                    Func<GameEvent, Task> onGameEventAsync;
                    lock (_mutex)
                    {
                        onGameEventAsync = _onGameEventAsync;
                    }

                    if (onGameEventAsync != null)
                    {
                        var gameEvent = _universe.GetCurrentGameEvent(_player);

                        if (!hasSentInitialValues)
                        {
                            hasSentInitialValues = true;
                            gameEvent.PhysicsValues = _universe.Mod.PhysicsValues;
                            gameEvent.ChunkData = _universe.ExampleChunkData;
                        }

                        await onGameEventAsync(gameEvent);
                    }

                    await Task.Delay(10);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _universe.DeregisterChannel(this);
            }
        }
    }
}
