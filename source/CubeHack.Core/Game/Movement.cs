﻿// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.Geometry;
using CubeHack.State;
using CubeHack.Util;
using System;

namespace CubeHack.Game
{
    internal static class Movement
    {
        public static PositionComponent Respawn(PositionComponent position)
        {
            position.Placement = new EntityPlacement(
                EntityPos.Origin + new EntityOffset((Rng.NextDouble() * 2 - 1) * 32, 0, (Rng.NextDouble() * 2 - 1) * 32),
                new EntityOrientation(0, Rng.NextDouble() * 2 * Math.PI));
            position.InternalPos = position.Placement.Pos;
            return position;
        }

        public static void MoveEntity(PhysicsValues physicsValues, World world, PositionComponent positionData, GameDuration elapsedDuration, EntityOffset offset)
        {
            offset.Y -= 0.5 * elapsedDuration.Seconds * physicsValues.Gravity;
            if (MoveY(physicsValues, world, positionData, offset.Y * elapsedDuration.Seconds))
            {
                positionData.IsFalling = false;
            }
            else
            {
                if (!positionData.IsFalling)
                {
                    /* Re-evaluate the fall from the apparent position. */
                    positionData.InternalPos = positionData.Placement.Pos;
                    MoveY(physicsValues, world, positionData, offset.Y * elapsedDuration.Seconds);
                    positionData.IsFalling = true;
                }
            }

            if (positionData.IsFalling)
            {
                offset.Y -= 0.5 * elapsedDuration.Seconds * physicsValues.Gravity;
            }
            else
            {
                if (Math.Abs(offset.Y) > Math.Sqrt(2 * physicsValues.Gravity * physicsValues.TerminalHeight))
                {
                    Respawn(positionData);
                }

                offset.Y = 0;
            }

            if (offset.X != 0 || offset.Z != 0)
            {
                long savedY = positionData.InternalPos.Y;
                if (!positionData.IsFalling && world[(BlockPos)positionData.InternalPos - BlockOffset.UnitY] != 0)
                {
                    // Temporarily move the character up a block so that it can climb up stairs.
                    MoveY(physicsValues, world, positionData, 1);
                }

                EntityPos positionBeforeMovement = positionData.InternalPos;
                double moveX = offset.X * elapsedDuration.Seconds;
                double moveZ = offset.Z * elapsedDuration.Seconds;

                if (offset.X > offset.Z)
                {
                    MoveX(physicsValues, world, positionData, moveX);
                    MoveZ(physicsValues, world, positionData, moveZ);
                }
                else
                {
                    MoveZ(physicsValues, world, positionData, moveZ);
                    MoveX(physicsValues, world, positionData, moveX);
                }

                var moveDelta = positionData.InternalPos - positionBeforeMovement;
                moveX -= moveDelta.X;
                moveZ -= moveDelta.Z;

                if (!positionData.IsFalling)
                {
                    // Attempt to move the character back down to the ground in case we didn't climb a stair.
                    MoveY(physicsValues, world, positionData, (savedY - positionData.InternalPos.Y) / (double)(1L << 32));

                    savedY = positionData.InternalPos.Y;

                    // Attempt to move the caracter down an additional block so that it can walk down stairs.
                    if (!MoveY(physicsValues, world, positionData, -((1L << 32) + 1) / (double)(1L << 32)))
                    {
                        positionData.InternalPos.Y = savedY;
                        positionData.IsFalling = true;
                    }
                }

                // Attempt to continue movement at this new (lower) Y position.
                if (offset.X > offset.Z)
                {
                    if (MoveX(physicsValues, world, positionData, moveX)) offset.X = 0;
                    if (MoveZ(physicsValues, world, positionData, moveZ)) offset.Z = 0;
                }
                else
                {
                    if (MoveZ(physicsValues, world, positionData, moveZ)) offset.Z = 0;
                    if (MoveX(physicsValues, world, positionData, moveX)) offset.X = 0;
                }
            }

            positionData.Velocity = offset;
            SetPositionFromCollisionPosition(physicsValues, world, positionData);
        }

        private static void SetPositionFromCollisionPosition(PhysicsValues physicsValues, World world, PositionComponent positionData)
        {
            if (positionData.IsFalling)
            {
                positionData.Placement.Pos = positionData.InternalPos;
                return;
            }

            var entityPos = positionData.InternalPos;
            var blockPos = (BlockPos)entityPos;

            /*
             * When standing on an edge, the entities bottom center might be hanging in the air.
             * Find the true floor position by iterating downward and looking for the floor.
             */
            if (world[blockPos] == 0 && world[blockPos - BlockOffset.UnitY] == 0)
            {
                blockPos -= BlockOffset.UnitY;
                entityPos -= EntityOffset.UnitY;

                if (world[blockPos - new BlockOffset(0, 2, 0)] == 0)
                {
                    blockPos -= BlockOffset.UnitY;
                    entityPos -= EntityOffset.UnitY;
                }
            }

            var entityOffset = entityPos - blockPos;

            double yOffset = 0;

            /*
             * Take the integral of all floor heights in a 2 by 2 area around the center.
             * Since the height is piecewise constant, this is a weighted sum of the heights around the entity position.
             */

            yOffset += (1 - entityOffset.X) * FindHeight(world, blockPos - BlockOffset.UnitX);
            yOffset += entityOffset.X * FindHeight(world, blockPos + BlockOffset.UnitX);
            yOffset += (1 - entityOffset.Z) * FindHeight(world, blockPos - BlockOffset.UnitZ);
            yOffset += entityOffset.Z * FindHeight(world, blockPos + BlockOffset.UnitZ);
            yOffset += (1 - entityOffset.X) * (1 - entityOffset.Z) * FindHeight(world, blockPos + new BlockOffset(-1, 0, -1));
            yOffset += (1 - entityOffset.X) * entityOffset.Z * FindHeight(world, blockPos + new BlockOffset(-1, 0, 1));
            yOffset += entityOffset.X * (1 - entityOffset.Z) * FindHeight(world, blockPos + new BlockOffset(1, 0, -1));
            yOffset += entityOffset.X * entityOffset.Z * FindHeight(world, blockPos + new BlockOffset(1, 0, 1));

            // Normalize.
            yOffset *= 0.25;

            entityPos += new EntityOffset(0, yOffset, 0);
            positionData.Placement.Pos = entityPos;
        }

        private static double FindHeight(World world, BlockPos blockPos)
        {
            if (world[blockPos] != 0) return 1;
            if (world[blockPos] == 0 && world[blockPos - BlockOffset.UnitY] == 0) return -1;
            return 0;
        }

        private static bool MoveX(PhysicsValues physicsValues, World world, PositionComponent positionData, double distance)
        {
            EntityPos position = positionData.InternalPos;
            long p = position.X;

            int cy0 = GetBlockCoordinate(position.Y + 1);
            int cy1 = GetBlockCoordinate(position.Y + (long)(physicsValues.PlayerHeight * (1L << 32)) - 1);
            int cz0 = GetBlockCoordinate(position.Z - (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) + 1);
            int cz1 = GetBlockCoordinate(position.Z + (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) - 1);
            bool hasCollided = MoveInternal(p, physicsValues.PlayerWidth * 0.5, distance, cx => AllPassable(world, new BlockPos(cx, cy0, cz0), new BlockPos(cx, cy1, cz1)), out p);
            position.X = p;

            positionData.InternalPos = position;
            return hasCollided;
        }

        private static bool MoveY(PhysicsValues physicsValues, World world, PositionComponent positionData, double distance)
        {
            EntityPos position = positionData.InternalPos;
            long p = position.Y;

            int cx0 = GetBlockCoordinate(position.X - (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) + 1);
            int cx1 = GetBlockCoordinate(position.X + (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) - 1);
            int cz0 = GetBlockCoordinate(position.Z - (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) + 1);
            int cz1 = GetBlockCoordinate(position.Z + (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) - 1);
            bool hasCollided = MoveInternal(p, distance > 0 ? physicsValues.PlayerHeight : 0, distance, cy => AllPassable(world, new BlockPos(cx0, cy, cz0), new BlockPos(cx1, cy, cz1)), out p);
            position.Y = p;

            positionData.InternalPos = position;
            return hasCollided;
        }

        private static bool MoveZ(PhysicsValues physicsValues, World world, PositionComponent positionData, double distance)
        {
            EntityPos position = positionData.InternalPos;
            long p = position.Z;

            int cx0 = GetBlockCoordinate(position.X - (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) + 1);
            int cx1 = GetBlockCoordinate(position.X + (long)(0.5 * physicsValues.PlayerWidth * (1L << 32)) - 1);
            int cy0 = GetBlockCoordinate(position.Y + 1);
            int cy1 = GetBlockCoordinate(position.Y + (long)(physicsValues.PlayerHeight * (1L << 32)) - 1);
            bool hasCollided = MoveInternal(p, physicsValues.PlayerWidth * 0.5, distance, cz => AllPassable(world, new BlockPos(cx0, cy0, cz), new BlockPos(cx1, cy1, cz)), out p);
            position.Z = p;

            positionData.InternalPos = position;
            return hasCollided;
        }

        private static bool MoveInternal(long startPosition, double radius, double distance, Func<int, bool> isPassable, out long position)
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

            int start = GetBlockCoordinate(startPosition + (1L << 31) * sign);
            int end = GetBlockCoordinate(startPosition + d * sign);

            for (int i = start; (i - end) * sign <= 0; i += sign)
            {
                if (!isPassable(i))
                {
                    long edge = (long)i << 32;
                    if (sign < 0)
                    {
                        edge += 1L << 32;
                    }
                    else
                    {
                        edge -= 1;
                    }

                    long edgeDistance = (edge - startPosition) * sign;

                    if (edgeDistance < 0)
                    {
                        // Attempt to move further inside the block.
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

        private static bool AllPassable(World world, BlockPos block0, BlockPos block1)
        {
            for (int x = block0.X; x <= block1.X; ++x)
            {
                for (int y = block0.Y; y <= block1.Y; ++y)
                {
                    for (int z = block0.Z; z <= block1.Z; ++z)
                    {
                        var blockPos = new BlockPos(x, y, z);
                        var chunk = world.GetChunk((ChunkPos)blockPos);

                        if (!chunk.IsCreated) return false;

                        if (world[blockPos] != 0)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static int GetBlockCoordinate(long entityCoordinate)
        {
            return (int)(entityCoordinate >> 32);
        }
    }
}
