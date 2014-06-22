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

        public PositionData PositionData = new PositionData();

        public PhysicsValues PhysicsValues = new PhysicsValues();

        public List<PositionData> EntityPositions = new List<PositionData>();

        public GameConnection(IChannel channel)
        {
            World = new World();

            _channel = channel;
            channel.OnGameEventAsync = HandleGameEventAsync;
        }

        public World World { get; private set; }

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
            PositionData.HAngle -= 0.1f * dx;
            if (PositionData.HAngle > 180)
            {
                PositionData.HAngle -= 360;
            }
            if (PositionData.HAngle < -180)
            {
                PositionData.HAngle += 360;
            }

            PositionData.VAngle += 0.1f * dy;
            if (PositionData.VAngle > 90)
            {
                PositionData.VAngle = 90;
            }
            if (PositionData.VAngle < -90)
            {
                PositionData.VAngle = -90;
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

                if (gameEvent.ChunkData != null)
                {
                    World.AddChunk(gameEvent.ChunkData);
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
                float lookZ = -elapsedTime * PhysicsValues.PlayerMovementSpeed * (float)Math.Cos(PositionData.HAngle * f);
                float lookX = -elapsedTime * PhysicsValues.PlayerMovementSpeed * (float)Math.Sin(PositionData.HAngle * f);

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

            if (vx > vz)
            {
                if (MoveX(vx)) vx = 0;
                if (MoveZ(vz)) vz = 0;
            }
            else
            {
                if (MoveZ(vz)) vz = 0;
                if (MoveX(vx)) vx = 0;
            }

            PositionData.Velocity = new Offset(vx, vy, vz);
            _channel.SendPlayerEvent(new PlayerEvent { PositionData = PositionData });
        }

        bool MoveX(double distance)
        {
            Position position = PositionData.Position;
            long p = position.X;

            int cy = position.CubeY;
            int cz0 = Position.GetCubeCoordinate(position.Z - (long)(0.5 * PhysicsValues.PlayerWidth * (1L << 32)) + 1);
            int cz1 = Position.GetCubeCoordinate(position.Z + (long)(0.5 * PhysicsValues.PlayerWidth * (1L << 32)) - 1);
            bool hasCollided = MoveInternal(p, PhysicsValues.PlayerWidth * 0.5, distance, cx => AllPassable(cx, cx, cy, cy, cz0, cz1), out p);
            position.X = p;

            PositionData.Position = position;
            return hasCollided;
        }

        bool MoveY(double distance)
        {
            Position position = PositionData.Position;
            long p = position.Y;

            int cx0 = Position.GetCubeCoordinate(position.X - (long)(0.5 * PhysicsValues.PlayerWidth * (1L << 32)) + 1);
            int cx1 = Position.GetCubeCoordinate(position.X + (long)(0.5 * PhysicsValues.PlayerWidth * (1L << 32)) - 1);
            int cz0 = Position.GetCubeCoordinate(position.Z - (long)(0.5 * PhysicsValues.PlayerWidth * (1L << 32)) + 1);
            int cz1 = Position.GetCubeCoordinate(position.Z + (long)(0.5 * PhysicsValues.PlayerWidth * (1L << 32)) - 1);
            bool hasCollided = MoveInternal(p, 0, distance, cy => AllPassable(cx0, cx1, cy, cy, cz0, cz1), out p);
            position.Y = p;

            PositionData.Position = position;
            return hasCollided;
        }

        bool MoveZ(double distance)
        {
            Position position = PositionData.Position;
            long p = position.Z;

            int cx0 = Position.GetCubeCoordinate(position.X - (long)(0.5 * PhysicsValues.PlayerWidth * (1L << 32)) + 1);
            int cx1 = Position.GetCubeCoordinate(position.X + (long)(0.5 * PhysicsValues.PlayerWidth * (1L << 32)) - 1);
            int cy = position.CubeY;
            bool hasCollided = MoveInternal(p, PhysicsValues.PlayerWidth * 0.5, distance, cz => AllPassable(cx0, cx1, cy, cy, cz, cz), out p);
            position.Z = p;

            PositionData.Position = position;
            return hasCollided;
        }

        bool MoveInternal(long startPosition, double radius, double distance, Func<int, bool> isPassable, out long position)
        {
            int sign;

            long d = (long)(distance * (1L << 32));
            long r = (long)(radius * (1L << 32));

            if (d == 0)
            {
                position = startPosition;
                return false;
            }
            else if (d > 0)
            {
                sign = 1;
            }
            else
            {
                sign = -1;
                d = -d;
            }

            startPosition += sign * r;

            int start = Position.GetCubeCoordinate(startPosition + (1L << 31) * sign);
            int end = Position.GetCubeCoordinate(startPosition + d * sign);

            for (int i = start; (i - end) * sign <= 0; i += sign)
            {
                if (!isPassable(i))
                {
                    long edge = (long)i << 32;
                    if (sign < 0)
                    {
                        edge += (1L << 32);
                    }
                    else
                    {
                        edge -= 1;
                    }

                    long edgeDistance = (edge - startPosition) * sign;

                    if (edgeDistance < 0)
                    {
                        // Attempt to move further inside the cube.
                        position = startPosition - sign * r;
                        return true;
                    }
                    else if (edgeDistance < d)
                    {
                        // Collision!
                        position = edge - sign * r;
                        return true;
                    }
                    else
                    {
                        // Can this even happen?
                        throw new Exception();
                    }
                }
            }

            position = startPosition + d * sign - sign * r;
            return false;
        }

        private int GetCurrentCubeCoordinate(long position, long direction)
        {
            return Position.GetCubeCoordinate(position + (1L << 31) * direction);
        }

        bool AllPassable(int cx0, int cx1, int cy0, int cy1, int cz0, int cz1)
        {
            for (int x = cx0; x <= cx1; ++x)
            {
                for (int y = cy0; y <= cy1; ++y)
                {
                    for (int z = cz0; z <= cz1; ++z)
                    {
                        if (World[x, y, z] != 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
