// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Game
{
    static class Movement
    {
        static readonly Random _random = new Random();

        public static void Respawn(PositionData positionData)
        {
            positionData.Position = new Position() + new Offset((_random.NextDouble() * 2 - 1) * 32, 0, (_random.NextDouble() * 2 - 1) * 32);
            positionData.CollisionPosition = positionData.Position;
        }

        public static void MoveEntity(PhysicsValues physicsValues, World world, PositionData positionData, double elapsedTime, double vx, double vy, double vz)
        {
            vy -= 0.5 * elapsedTime * physicsValues.Gravity;
            if (MoveY(physicsValues, world, positionData, vy * elapsedTime))
            {
                positionData.IsFalling = false;

                if (Math.Abs(vy) > Math.Sqrt(2 * physicsValues.Gravity * physicsValues.TerminalHeight))
                {
                    Respawn(positionData);
                }

                vy = 0;
            }
            else
            {
                vy -= 0.5 * elapsedTime * physicsValues.Gravity;
                positionData.IsFalling = true;
            }

            if (vx != 0 || vz != 0)
            {
                long savedY = 0;
                if (!positionData.IsFalling)
                {
                    // Temporarily move the character up a cube so that it can climb up stairs.
                    savedY = positionData.CollisionPosition.Y;
                    MoveY(physicsValues, world, positionData, 1);
                }

                Position positionBeforeMovement = positionData.CollisionPosition;
                double moveX = vx * elapsedTime;
                double moveZ = vz * elapsedTime;

                if (vx > vz)
                {
                    MoveX(physicsValues, world, positionData, moveX);
                    MoveZ(physicsValues, world, positionData, moveZ);
                }
                else
                {
                    MoveZ(physicsValues, world, positionData, moveZ);
                    MoveX(physicsValues, world, positionData, moveX);
                }

                var moveDelta = positionData.CollisionPosition - positionBeforeMovement;
                moveX -= moveDelta.X;
                moveZ -= moveDelta.Z;

                if (!positionData.IsFalling)
                {
                    // Attempt to move the character back down to the ground in case we didn't climb a stair.
                    MoveY(physicsValues, world, positionData, (savedY - positionData.CollisionPosition.Y) / (double)(1L << 32));

                    savedY = positionData.CollisionPosition.Y;

                    // Attempt to move the caracter down an additional cube so that it can walk down stairs.
                    if (!MoveY(physicsValues, world, positionData, -((1L << 32) + 1) / (double)(1L << 32)))
                    {
                        positionData.CollisionPosition.Y = savedY;
                        positionData.IsFalling = true;
                    }
                }

                // Attempt to continue movement at this new (lower) Y position.
                if (vx > vz)
                {
                    if (MoveX(physicsValues, world, positionData, moveX)) vx = 0;
                    if (MoveZ(physicsValues, world, positionData, moveZ)) vz = 0;
                }
                else
                {
                    if (MoveZ(physicsValues, world, positionData, moveZ)) vz = 0;
                    if (MoveX(physicsValues, world, positionData, moveX)) vx = 0;
                }
            }

            positionData.Velocity = new Offset(vx, vy, vz);
            SetPositionFromCollisionPosition(physicsValues, world, positionData);
        }

        static void SetPositionFromCollisionPosition(PhysicsValues physicsValues, World world, PositionData positionData)
        {
            if (positionData.IsFalling)
            {
                positionData.Position = positionData.CollisionPosition;
                return;
            }

            var position = positionData.CollisionPosition;

            double yOffset = 0;

            double radius = 0.5 - Offset.Epsilon;
            Position p;

            double weight = 0;
            for (int x = -8; x <= 8; ++x)
            {
                for (int z = -8; z <= 8; ++z)
                {
                    p = position + new Offset(radius * 0.25 * x, 0, radius * 0.25 * z);

                    double w = (x >= -4 && x <= 4 && z >= -4 && z <= 4) ? 1 : 0.5;
                    yOffset += w * GetWeightedOffset(world, p);
                    weight += w;
                }
            }

            yOffset /= weight;

            position += new Offset(0, -yOffset, 0);
            positionData.Position = position;
        }

        private static double GetWeightedOffset(World world, Position p)
        {
            if (world[p.CubeX, p.CubeY - 1, p.CubeZ] == 0)
            {
                if (world[p.CubeX, p.CubeY - 2, p.CubeZ] == 0)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }

            return 0;
        }

        static long MaxMin(long a, long b, long c)
        {
            return Math.Max(a, Math.Min(b, c));
        }

        static bool IsAllowed(PhysicsValues physicsValues, World world, Position position)
        {
            var a = position + new Offset(Offset.Epsilon, 0, Offset.Epsilon);
            var b = position + new Offset(-Offset.Epsilon, physicsValues.PlayerHeight - Offset.Epsilon, -Offset.Epsilon);
            return AllPassable(world, a.CubeX, b.CubeX, a.CubeY, b.CubeY, a.CubeZ, b.CubeZ);
        }

        static bool MoveX(PhysicsValues physicsValues, World world, PositionData positionData, double distance)
        {
            Position position = positionData.CollisionPosition;
            long p = position.X;

            int cy0 = Position.GetCubeCoordinate(position.Y + 1);
            int cy1 = Position.GetCubeCoordinate(position.Y + (long)(physicsValues.PlayerHeight * (1L << 32)) - 1);
            int cz0 = Position.GetCubeCoordinate(position.Z - (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) + 1);
            int cz1 = Position.GetCubeCoordinate(position.Z + (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) - 1);
            bool hasCollided = MoveInternal(p, physicsValues.PlayerWidth * 0.5, distance, cx => AllPassable(world, cx, cx, cy0, cy1, cz0, cz1), out p);
            position.X = p;

            positionData.CollisionPosition = position;
            return hasCollided;
        }

        static bool MoveY(PhysicsValues physicsValues, World world, PositionData positionData, double distance)
        {
            Position position = positionData.CollisionPosition;
            long p = position.Y;

            int cx0 = Position.GetCubeCoordinate(position.X - (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) + 1);
            int cx1 = Position.GetCubeCoordinate(position.X + (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) - 1);
            int cz0 = Position.GetCubeCoordinate(position.Z - (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) + 1);
            int cz1 = Position.GetCubeCoordinate(position.Z + (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) - 1);
            bool hasCollided = MoveInternal(p, distance > 0 ? physicsValues.PlayerHeight : 0, distance, cy => AllPassable(world, cx0, cx1, cy, cy, cz0, cz1), out p);
            position.Y = p;

            positionData.CollisionPosition = position;
            return hasCollided;
        }

        static bool MoveZ(PhysicsValues physicsValues, World world, PositionData positionData, double distance)
        {
            Position position = positionData.CollisionPosition;
            long p = position.Z;

            int cx0 = Position.GetCubeCoordinate(position.X - (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) + 1);
            int cx1 = Position.GetCubeCoordinate(position.X + (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) - 1);
            int cy0 = Position.GetCubeCoordinate(position.Y + 1);
            int cy1 = Position.GetCubeCoordinate(position.Y + (long)(physicsValues.PlayerHeight * (1L << 32)) - 1);
            bool hasCollided = MoveInternal(p, physicsValues.PlayerWidth * 0.5, distance, cz => AllPassable(world, cx0, cx1, cy0, cy1, cz, cz), out p);
            position.Z = p;

            positionData.CollisionPosition = position;
            return hasCollided;
        }

        static bool MoveInternal(long startPosition, double radius, double distance, Func<int, bool> isPassable, out long position)
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

        static bool AllPassable(World world, int cx0, int cx1, int cy0, int cy1, int cz0, int cz1)
        {
            for (int x = cx0; x <= cx1; ++x)
            {
                for (int y = cy0; y <= cy1; ++y)
                {
                    for (int z = cz0; z <= cz1; ++z)
                    {
                        if (world[x, y, z] != 0)
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
