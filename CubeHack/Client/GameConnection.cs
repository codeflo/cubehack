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
        public float PlayerAngleH = 0, PlayerAngleV = 0;

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

        public IDisposable TakeRenderLock()
        {
            var unlocker = _mutex.TakePriorityLock();
            return unlocker;
        }

        public void MouseLook(float dx, float dy)
        {
            PlayerAngleH -= 0.1f * dx;
            if (PlayerAngleH > 180)
            {
                PlayerAngleH -= 360;
            }
            if (PlayerAngleH < -180)
            {
                PlayerAngleH += 360;
            }

            PlayerAngleV += 0.1f * dy;
            if (PlayerAngleV > 90)
            {
                PlayerAngleV = 90;
            }
            if (PlayerAngleV < -90)
            {
                PlayerAngleV = -90;
            }
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

        public void UpdateState(bool hasFocus, bool mouseLookActive)
        {
            float elapsedTime = _frameTimer.SetZero();

            var keyboardState = Keyboard.GetState();

            float vx = 0, vy = 0, vz = 0;

            if (hasFocus)
            {
                float f = (float)Math.PI / 180.0f;
                float lookZ = -elapsedTime * PhysicsValues.PlayerMovementSpeed * (float)Math.Cos(PlayerAngleH * f);
                float lookX = -elapsedTime * PhysicsValues.PlayerMovementSpeed * (float)Math.Sin(PlayerAngleH * f);

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

            PlayerX += vx;
            PlayerY += vy;
            PlayerZ += vz;
            _channel.SendPlayerEvent(new PlayerEvent { X = PlayerX, Y = PlayerY, Z = PlayerZ, VX = vx, VY = vy, VZ = vz });
        }
    }
}
