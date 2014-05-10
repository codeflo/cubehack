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
        long _gameEventTicks = 0;

        public float PlayerX, PlayerY, PlayerZ;

        public List<GameEvent.EntityData> Entities = new List<GameEvent.EntityData>();

        public GameConnection(IChannel channel)
        {
            _channel = channel;
            channel.OnGameEventAsync = HandleGameEventAsync;
        }

        public float TimeSinceGameEvent
        {
            get
            {
                return (_stopwatch.ElapsedTicks - _gameEventTicks) / (float)Stopwatch.Frequency;
            }
        }

        public IDisposable TakeRenderLock(bool hasFocus)
        {
            var unlocker = _mutex.TakePriorityLock();

            UpdateState(hasFocus);

            return unlocker;
        }

        Task HandleGameEventAsync(GameEvent gameEvent)
        {
            using (_mutex.TakeLock())
            {
                // We extrapolate from this, so using server time would be more accurate perhaps?
                _gameEventTicks = _stopwatch.ElapsedTicks;

                Entities = gameEvent.Entities ?? new List<GameEvent.EntityData>();
            }

            return Task.FromResult(0);
        }

        void UpdateState(bool hasFocus)
        {
            long currentTicks = _stopwatch.ElapsedTicks;
            float elapsedTime = (currentTicks - _lastFrameTicks) / (float)Stopwatch.Frequency;
            _lastFrameTicks = currentTicks;

            var keyboardState = Keyboard.GetState();

            float vx = 0, vy = 0, vz = 0;

            if (hasFocus)
            {
                if (keyboardState.IsKeyDown(Key.W))
                {
                    vz = -elapsedTime * MovementSpeed;
                }

                if (keyboardState.IsKeyDown(Key.A))
                {
                    vx = -elapsedTime * MovementSpeed;
                }

                if (keyboardState.IsKeyDown(Key.S))
                {
                    vz = elapsedTime * MovementSpeed;
                }

                if (keyboardState.IsKeyDown(Key.D))
                {
                    vx = elapsedTime * MovementSpeed;
                }
            }

            PlayerX += vx;
            PlayerY += vy;
            PlayerZ += vz;
            _channel.SendPlayerEvent(new PlayerEvent { X = PlayerX, Y = PlayerY, Z = PlayerZ, VX = vx, VY = vy, VZ = vz });
        }
    }
}
