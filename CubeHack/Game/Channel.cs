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
        readonly Entity _player;

        public Channel(Entity player)
        {
            _player = player;
        }

        public event Action<GameEvent> GameEvent;

        public Entity Player
        {
            get { return _player; }
        }

        public void SendPlayerEvent(PlayerEvent playerEvent)
        {
            lock (_player)
            {
                _player.X = playerEvent.X;
                _player.Y = playerEvent.Y;
                _player.Z = playerEvent.Z;
            }
        }

        public void RaiseGameEvent(GameEvent gameEvent)
        {
            var gameEventHandler = GameEvent;
            if (gameEventHandler != null)
            {
                Task.Run(() => gameEventHandler(gameEvent));
            }
        }
    }
}
