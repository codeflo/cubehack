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

        public PositionData Position = new PositionData();

        public PhysicsValues PhysicsValues = new PhysicsValues();

        public List<PositionData> EntityPositions = new List<PositionData>();

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

        public IDisposable TakeRenderLock()
        {
            var unlocker = _mutex.TakePriorityLock();
            return unlocker;
        }

        public void MouseLook(float dx, float dy)
        {
            Position.HAngle -= 0.1f * dx;
            if (Position.HAngle > 180)
            {
                Position.HAngle -= 360;
            }
            if (Position.HAngle < -180)
            {
                Position.HAngle += 360;
            }

            Position.VAngle += 0.1f * dy;
            if (Position.VAngle > 90)
            {
                Position.VAngle = 90;
            }
            if (Position.VAngle < -90)
            {
                Position.VAngle = -90;
            }
        }

        Task HandleGameEventAsync(GameEvent gameEvent)
        {
            using (_mutex.TakeLock())
            {
                // We extrapolate from this, so using server time would be more accurate perhaps?
                _gameEventTimer.SetZero();

                EntityPositions = gameEvent.EntityPositions ?? new List<PositionData>();

                if (gameEvent.PhysicsValues != null)
                {
                    PhysicsValues = gameEvent.PhysicsValues;
                }
            }

            return Task.FromResult(0);
        }

        public void UpdateState(bool hasFocus, bool mouseLookActive)
        {
            float elapsedTime = _frameTimer.SetZero();

            var keyboardState = Keyboard.GetState();

            float vx = 0, vy = 0, vz = 0;

            if (hasFocus)
            {
                float f = (float)Math.PI / 180.0f;
                float lookZ = -elapsedTime * PhysicsValues.PlayerMovementSpeed * (float)Math.Cos(Position.HAngle * f);
                float lookX = -elapsedTime * PhysicsValues.PlayerMovementSpeed * (float)Math.Sin(Position.HAngle * f);

                if (keyboardState.IsKeyDown(Key.W))
                {
                    vx += lookX;
                    vz += lookZ;
                }

                if (keyboardState.IsKeyDown(Key.A))
                {
                    vx += lookZ;
                    vz -= lookX;
                }

                if (keyboardState.IsKeyDown(Key.S))
                {
                    vx -= lookX;
                    vz -= lookZ;
                }

                if (keyboardState.IsKeyDown(Key.D))
                {
                    vx -= lookZ;
                    vz += lookX;
                }
            }

            Position.X += vx;
            Position.Y += vy;
            Position.Z += vz;
            _channel.SendPlayerEvent(new PlayerEvent { Position = Position });
        }
    }
}
