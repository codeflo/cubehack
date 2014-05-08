// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Game;
using CubeHack.Util;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Client
{
    sealed class GameConnection
    {
        const float MovementSpeed = 5f;

        readonly PriorityMutex _mutex = new PriorityMutex();
        readonly IChannel _channel;

        readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        long _lastFrameTicks = 0;

        public float PlayerX, PlayerY, PlayerZ;

        public List<GameEvent.EntityData> Entities = new List<GameEvent.EntityData>();

        public GameConnection(IChannel channel)
        {
            _channel = channel;
            channel.GameEvent += HandleGameEvent;
        }

        public IDisposable TakeRenderLock(bool hasFocus)
        {
            var unlocker = _mutex.TakePriorityLock();

            UpdateState(hasFocus);

            return unlocker;
        }

        public void HandleGameEvent(GameEvent gameEvent)
        {
            using (_mutex.TakeLock())
            {
                Entities = gameEvent.Entities ?? new List<GameEvent.EntityData>();
            }
        }

        void UpdateState(bool hasFocus)
        {
            long currentTicks = _stopwatch.ElapsedTicks;
            float elapsedTime = (currentTicks - _lastFrameTicks) / (float)Stopwatch.Frequency;
            _lastFrameTicks = currentTicks;

            var keyboardState = Keyboard.GetState();

            if (hasFocus)
            {
                if (keyboardState.IsKeyDown(Key.W))
                {
                    PlayerZ -= elapsedTime * MovementSpeed;
                }

                if (keyboardState.IsKeyDown(Key.A))
                {
                    PlayerX -= elapsedTime * MovementSpeed;
                }

                if (keyboardState.IsKeyDown(Key.S))
                {
                    PlayerZ += elapsedTime * MovementSpeed;
                }

                if (keyboardState.IsKeyDown(Key.D))
                {
                    PlayerX += elapsedTime * MovementSpeed;
                }
            }

            Task.Run(() => _channel.SendPlayerEvent(new PlayerEvent { X = PlayerX, Y = PlayerY, Z = PlayerZ })).Forget();
        }
    }
}
