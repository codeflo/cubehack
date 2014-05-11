// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Game;
using CubeHack.GameData;
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
        readonly PriorityMutex _mutex = new PriorityMutex();
        readonly IChannel _channel;

        readonly PrecisionTimer _frameTimer = new PrecisionTimer();
        readonly PrecisionTimer _gameEventTimer = new PrecisionTimer();

        public float PlayerX, PlayerY, PlayerZ;

        public PhysicsValues PhysicsValues = new PhysicsValues();

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
                return _gameEventTimer.ElapsedTime;
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
                _gameEventTimer.SetZero();

                Entities = gameEvent.Entities ?? new List<GameEvent.EntityData>();

                if (gameEvent.PhysicsValues != null)
                {
                    PhysicsValues = gameEvent.PhysicsValues;
                }
            }

            return Task.FromResult(0);
        }

        void UpdateState(bool hasFocus)
        {
            float elapsedTime = _gameEventTimer.SetZero();

            var keyboardState = Keyboard.GetState();

            float vx = 0, vy = 0, vz = 0;

            if (hasFocus)
            {
                if (keyboardState.IsKeyDown(Key.W))
                {
                    vz = -elapsedTime * PhysicsValues.PlayerMovementSpeed;
                }

                if (keyboardState.IsKeyDown(Key.A))
                {
                    vx = -elapsedTime * PhysicsValues.PlayerMovementSpeed;
                }

                if (keyboardState.IsKeyDown(Key.S))
                {
                    vz = elapsedTime * PhysicsValues.PlayerMovementSpeed;
                }

                if (keyboardState.IsKeyDown(Key.D))
                {
                    vx = elapsedTime * PhysicsValues.PlayerMovementSpeed;
                }
            }

            PlayerX += vx;
            PlayerY += vy;
            PlayerZ += vz;
            _channel.SendPlayerEvent(new PlayerEvent { X = PlayerX, Y = PlayerY, Z = PlayerZ, VX = vx, VY = vy, VZ = vz });
        }
    }
}
