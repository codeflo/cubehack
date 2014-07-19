// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Data;
using CubeHack.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    public abstract class AbstractGameClient
    {
        readonly PriorityMutex _mutex = new PriorityMutex();
        readonly IChannel _channel;

        readonly PrecisionTimer _frameTimer = new PrecisionTimer();

        readonly double _inverseSqrt2 = Math.Sqrt(0.5);

        static readonly Random _random = new Random();

        double _miningTime = 0;
        double _placementCooldown = 0;

        List<CubeUpdateData> _cubeUpdates = new List<CubeUpdateData>();

        public PositionData PositionData = new PositionData();

        public PhysicsValues PhysicsValues = new PhysicsValues();

        public List<PositionData> EntityPositions = new List<PositionData>();

        public bool IsFrozen { get; set; }

        public AbstractGameClient(IChannel channel)
        {
            World = new World();
            IsFrozen = true;

            ResetPosition();

            _channel = channel;
            channel.OnGameEventAsync = HandleGameEventAsync;

            Task.Run(async () =>
                {
                    PlayerEvent playerEvent;
                    using (_mutex.TakeLock())
                    {
                        playerEvent = CreatePlayerEvent();
                    }

                    await _channel.SendPlayerEventAsync(playerEvent);
                });
        }

        private void ResetPosition()
        {
            Movement.Respawn(PositionData);
        }

        public World World { get; private set; }

        public RayCastResult HighlightedCube { get; private set; }

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

        internal virtual void OnSendPlayerEvents()
        {
        }

        Task HandleGameEventAsync(GameEvent gameEvent)
        {
            PlayerEvent playerEvent;

            using (_mutex.TakeLock())
            {
                if (gameEvent.EntityPositions != null)
                {
                    EntityPositions = gameEvent.EntityPositions;
                }

                if (gameEvent.PhysicsValues != null)
                {
                    PhysicsValues = gameEvent.PhysicsValues;
                }

                if (gameEvent.ChunkDataList != null)
                {
                    foreach (var chunkData in gameEvent.ChunkDataList)
                    {
                        World.PasteChunkData(chunkData);
                    }
                }

                if (gameEvent.CubeUpdates != null)
                {
                    foreach (var chunkUpdate in gameEvent.CubeUpdates)
                    {
                        World[chunkUpdate.X, chunkUpdate.Y, chunkUpdate.Z] = chunkUpdate.Material;
                    }
                }

                if (gameEvent.IsFrozen != null)
                {
                    IsFrozen = gameEvent.IsFrozen.Value;
                }

                OnSendPlayerEvents();
                playerEvent = CreatePlayerEvent();
            }

            return _channel.SendPlayerEventAsync(playerEvent);
        }

        protected void UpdateState(Func<GameKey, bool> isKeyPressed)
        {
            if (IsFrozen)
            {
                return;
            }

            double elapsedTime = _frameTimer.SetZero();
            MovePlayer(elapsedTime, isKeyPressed);

            foreach (var entityPosition in EntityPositions)
            {
                Movement.MoveEntity(PhysicsValues, World, entityPosition, elapsedTime, entityPosition.Velocity.X, entityPosition.Velocity.Y, entityPosition.Velocity.Z);
            }

            UpdateBuildAction(elapsedTime, isKeyPressed);
        }

        private PlayerEvent CreatePlayerEvent()
        {
            var playerEvent = new PlayerEvent();

            playerEvent.PositionData = new PositionData
            {
                Position = PositionData.Position,
                CollisionPosition = PositionData.CollisionPosition,
                Velocity = PositionData.Velocity,
                HAngle = PositionData.HAngle,
                VAngle = PositionData.VAngle,
                IsFalling = PositionData.IsFalling,
            };

            if (_cubeUpdates.Count > 0)
            {
                playerEvent.CubeUpdates = _cubeUpdates;
                _cubeUpdates = new List<CubeUpdateData>();
            }
            return playerEvent;
        }

        private void UpdateBuildAction(double elapsedTime, Func<GameKey, bool> isKeyPressed)
        {
            if (_placementCooldown > 0)
            {
                _placementCooldown -= elapsedTime;
            }

            double f = Math.PI / 180.0;
            double lookZ = -Math.Cos(PositionData.HAngle * f) * Math.Cos(-PositionData.VAngle * f);
            double lookX = -Math.Sin(PositionData.HAngle * f) * Math.Cos(-PositionData.VAngle * f);
            double lookY = Math.Sin(-PositionData.VAngle * f);

            var result = World.CastRay(PositionData.Position + new Offset(0, PhysicsValues.PlayerEyeHeight, 0), new Offset(lookX, lookY, lookZ), PhysicsValues.MiningDistance);
            if (result == null)
            {
                HighlightedCube = null;
                _miningTime = 0;
            }
            else
            {
                if (RayCastResult.FaceEquals(HighlightedCube, result)
                    && isKeyPressed(GameKey.Primary))
                {
                    _miningTime += elapsedTime;

                    if (_miningTime >= PhysicsValues.MiningTime)
                    {
                        _cubeUpdates.Add(new CubeUpdateData
                            {
                                X = result.CubeX,
                                Y = result.CubeY,
                                Z = result.CubeZ,
                                Material = 0,
                            });
                    }
                }
                else
                {
                    _miningTime = 0;

                    if (_placementCooldown <= 0 && isKeyPressed(GameKey.Secondary))
                    {
                        _placementCooldown = PhysicsValues.PlacementCooldown;
                        _cubeUpdates.Add(new CubeUpdateData
                            {
                                X = result.CubeX + result.NormalX,
                                Y = result.CubeY + result.NormalY,
                                Z = result.CubeZ + result.NormalZ,
                                Material = 1,
                            });
                    }
                }

                HighlightedCube = result;
            }
        }

        private void MovePlayer(double elapsedTime, Func<GameKey, bool> isKeyPressed)
        {
            double vx = 0, vz = 0, vy = PositionData.Velocity.Y;

            double f = Math.PI / 180.0;
            double lookZ = -PhysicsValues.PlayerMovementSpeed * Math.Cos(PositionData.HAngle * f);
            double lookX = -PhysicsValues.PlayerMovementSpeed * Math.Sin(PositionData.HAngle * f);

            bool isMovingAlong = false, isMovingSideways = false;
            if (isKeyPressed(GameKey.Forwards))
            {
                isMovingAlong = true;
                vx += lookX;
                vz += lookZ;
            }

            if (isKeyPressed(GameKey.Left))
            {
                isMovingSideways = true;
                vx += lookZ;
                vz -= lookX;
            }

            if (isKeyPressed(GameKey.Backwards))
            {
                isMovingAlong = true;
                vx -= lookX;
                vz -= lookZ;
            }

            if (isKeyPressed(GameKey.Right))
            {
                isMovingSideways = true;
                vx -= lookZ;
                vz += lookX;
            }

            if (isMovingAlong && isMovingSideways)
            {
                vx *= _inverseSqrt2;
                vz *= _inverseSqrt2;
            }

            if (!PositionData.IsFalling && isKeyPressed(GameKey.Jump))
            {
                vy += GetJumpingSpeed();
            }

            Movement.MoveEntity(PhysicsValues, World, PositionData, elapsedTime, vx, vy, vz);
        }

        private double GetJumpingSpeed()
        {
            return Math.Sqrt(2 * PhysicsValues.Gravity * PhysicsValues.PlayerJumpHeight);
        }

        private int GetCurrentCubeCoordinate(long position, long direction)
        {
            return Position.GetCubeCoordinate(position + (1L << 31) * direction);
        }
    }
}
