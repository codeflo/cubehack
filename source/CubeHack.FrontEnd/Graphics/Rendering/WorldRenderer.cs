// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.FrontEnd.Graphics.Engine;
using CubeHack.Game;
using CubeHack.Geometry;
using CubeHack.Util;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

using static CubeHack.Geometry.GeometryConstants;

namespace CubeHack.FrontEnd.Graphics.Rendering
{
    internal sealed class WorldRenderer
    {
        private const float _viewingAngle = 100f;

        private static readonly double ChunkRadius = Math.Sqrt(3 * ExtraMath.Square((double)Chunk.Size));
        private static readonly GameDuration _chunkCacheExpiration = GameDuration.FromSeconds(30);

        private static readonly VertexSpecification _cubeVertexSpecification = new VertexSpecification()
        {
            { 0, 3, VertexAttribPointerType.Float, false },
            { 1, 2, VertexAttribPointerType.UnsignedShort, true },
            { 2, 4, VertexAttribPointerType.UnsignedByte, true },
        };

        private readonly WorldTextureAtlas _textureAtlas;
        private readonly OutlineRenderer _outlineRenderer;

        private readonly Lazy<Shader> _cubeShader = new Lazy<Shader>(() => Shader.Load("CubeHack.FrontEnd.Shaders.Cube"));
        private readonly Dictionary<ChunkPos, ChunkBufferEntry> _chunkBuffers = new Dictionary<ChunkPos, ChunkBufferEntry>();

        private readonly TriangleBuffer _tempTriangles = new TriangleBuffer(_cubeVertexSpecification);

        private Matrix4 _projectionMatrix;
        private Matrix4 _modelViewMatrix;

        private GameTime _currentFrameTime;

        public WorldRenderer(WorldTextureAtlas textureAtlas, OutlineRenderer outlineRenderer)
        {
            _textureAtlas = textureAtlas;
            _outlineRenderer = outlineRenderer;
        }

        public void Render(GameClient gameClient, RenderInfo renderInfo)
        {
            _currentFrameTime = GameTime.Now();

            GL.ClearColor(0.5f, 0.6f, 0.9f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            var offset = gameClient.PositionData.Position - new EntityPos();
            SetProjectionMatrix(renderInfo);

            _modelViewMatrix = Matrix4.Identity
                * Matrix4.CreateTranslation((float)-offset.X, (float)-offset.Y - gameClient.PhysicsValues.PlayerEyeHeight, (float)-offset.Z)
                * Matrix4.CreateRotationY(-gameClient.PositionData.HAngle * (float)ExtraMath.RadiansPerDegree)
                * Matrix4.CreateRotationX(gameClient.PositionData.VAngle * (float)ExtraMath.RadiansPerDegree);

            _textureAtlas.Bind();

            RenderHighlightedFace(gameClient);

            RenderCubes(gameClient);

            RenderEntities(gameClient);

            _outlineRenderer.RenderOutlines(renderInfo);
        }

        private void RenderHighlightedFace(GameClient gameClient)
        {
            var highlightedCube = gameClient.HighlightedCube;
            if (highlightedCube == null) return;

            var textureEntry = _textureAtlas.GetTextureEntry(gameClient.World[new BlockPos(highlightedCube.CubeX, highlightedCube.CubeY, highlightedCube.CubeZ)]);

            _tempTriangles.Clear();

            float highlight = 0.2f;
            if (highlightedCube.NormalX < 0) DrawCubeLeft(_tempTriangles, textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f, highlight);
            if (highlightedCube.NormalX > 0) DrawCubeRight(_tempTriangles, textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f, highlight);
            if (highlightedCube.NormalY < 0) DrawCubeBottom(_tempTriangles, textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f, highlight);
            if (highlightedCube.NormalY > 0) DrawCubeTop(_tempTriangles, textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f, highlight);
            if (highlightedCube.NormalZ < 0) DrawCubeBack(_tempTriangles, textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f, highlight);
            if (highlightedCube.NormalZ > 0) DrawCubeFront(_tempTriangles, textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f, highlight);

            GL.UseProgram(_cubeShader.Value.Id);

            GL.UniformMatrix4(0, false, ref _projectionMatrix);
            GL.UniformMatrix4(4, false, ref _modelViewMatrix);

            using (var vertexArray = new VertexArray(_cubeVertexSpecification))
            {
                vertexArray.SetData(_tempTriangles, BufferUsageHint.DynamicDraw);
                vertexArray.Draw();
            }

            GL.UseProgram(0);
        }

        private void RenderEntities(GameClient gameClient)
        {
            var positions = gameClient.EntityPositions;
            if (positions == null) return;

            GL.UseProgram(_cubeShader.Value.Id);

            GL.UniformMatrix4(0, false, ref _projectionMatrix);
            GL.UniformMatrix4(4, false, ref _modelViewMatrix);

            foreach (var position in positions)
            {
                var offset2 = position.Position - new EntityPos();
                GL.Uniform3(12, (float)offset2.X, (float)offset2.Y, (float)offset2.Z);

                using (var vertexArray = new VertexArray(_cubeVertexSpecification))
                {
                    _tempTriangles.Clear();
                    DrawEntity(_tempTriangles, 0, 0, 0, 0.5f * gameClient.PhysicsValues.PlayerWidth, gameClient.PhysicsValues.PlayerHeight);
                    vertexArray.SetData(_tempTriangles, BufferUsageHint.DynamicDraw);
                    vertexArray.Draw();
                }

                GL.Uniform3(12, 0f, 0f, 0f);
            }

            GL.UseProgram(0);
        }

        private void RenderCubes(GameClient gameClient)
        {
            GL.UseProgram(_cubeShader.Value.Id);

            GL.UniformMatrix4(0, false, ref _projectionMatrix);
            GL.UniformMatrix4(4, false, ref _modelViewMatrix);

            int chunkX = gameClient.PositionData.Position.CubeX >> Chunk.Bits;
            int chunkY = gameClient.PositionData.Position.CubeY >> Chunk.Bits;
            int chunkZ = gameClient.PositionData.Position.CubeZ >> Chunk.Bits;

            var offset = (gameClient.PositionData.Position - new EntityPos()) + new EntityOffset(0, gameClient.PhysicsValues.PlayerEyeHeight, 0);

            int chunkUpdates = 0;

            for (int y = chunkY + ChunkViewRadiusY; y >= chunkY - ChunkViewRadiusY; --y)
            {
                for (int x = chunkX - ChunkViewRadiusXZ; x <= chunkX + ChunkViewRadiusXZ; ++x)
                {
                    for (int z = chunkZ - ChunkViewRadiusXZ; z <= chunkZ + ChunkViewRadiusXZ; ++z)
                    {
                        var chunkPos = new ChunkPos(x, y, z);

                        ChunkBufferEntry entry;
                        _chunkBuffers.TryGetValue(chunkPos, out entry);

                        /* Don't let nearby entries expire. */
                        if (entry != null) entry.LastAccess = _currentFrameTime;

                        if (!IsInViewingFrustum(gameClient, offset, x, y, z)) continue;

                        var chunk = gameClient.World.PeekChunk(chunkPos);
                        if (chunk != null && chunk.HasData)
                        {
                            if (entry == null || (entry.ContentHash != chunk.ContentHash))
                            {
                                ++chunkUpdates;

                                VertexArray vertexArray;
                                if (entry == null)
                                {
                                    vertexArray = new VertexArray(_cubeVertexSpecification);
                                    entry = new ChunkBufferEntry { ContentHash = chunk.ContentHash, VertexArray = vertexArray, LastAccess = _currentFrameTime };
                                    _chunkBuffers[chunkPos] = entry;
                                }
                                else
                                {
                                    vertexArray = entry.VertexArray;
                                    entry.ContentHash = chunk.ContentHash;
                                }

                                _tempTriangles.Clear();
                                RenderChunk(_tempTriangles, chunk, x, y, z);
                                vertexArray.SetData(_tempTriangles, BufferUsageHint.StaticDraw);
                            }

                            entry.VertexArray.Draw();
                        }
                    }
                }
            }

            GL.UseProgram(0);

            RemoveExpiredChunks();
        }

        private void RemoveExpiredChunks()
        {
            HashSet<ChunkPos> keysToRemove = null;
            foreach (var entry in _chunkBuffers)
            {
                if (_currentFrameTime - entry.Value.LastAccess > _chunkCacheExpiration)
                {
                    entry.Value.VertexArray.Dispose();
                    if (keysToRemove == null) keysToRemove = new HashSet<ChunkPos>();
                    keysToRemove.Add(entry.Key);
                }
            }

            if (keysToRemove != null)
            {
                foreach (var entry in keysToRemove)
                {
                    _chunkBuffers.Remove(entry);
                }
            }
        }

        private bool IsInViewingFrustum(GameClient gameClient, EntityOffset offset, int x, int y, int z)
        {
            // Check if the bounding sphere of the chunk is in the viewing frustum.

            double df = (double)(Chunk.Size);

            // Determine the chunk center relative to the viewer.
            double dx = (x + 0.5) * df - offset.X;
            double dy = (y + 0.5) * df - offset.Y;
            double dz = (z + 0.5) * df - offset.Z;

            double t0, t1;

            // Perform mouselook rotation
            double ha = gameClient.PositionData.HAngle * ExtraMath.RadiansPerDegree;
            double hc = Math.Cos(ha), hs = Math.Sin(ha);
            t0 = dx * hc - dz * hs; t1 = dz * hc + dx * hs; dx = t0; dz = t1;

            double va = -gameClient.PositionData.VAngle * ExtraMath.RadiansPerDegree;
            double vc = Math.Cos(va), vs = Math.Sin(va);
            t0 = dz * vc - dy * vs; t1 = dy * vc + dz * vs; dz = t0; dy = t1;

            // Check if the chunk is behind the viewer.
            if (dz > ChunkRadius && dz > ChunkRadius) return false;

            // TODO: We can discard even more chunks by taking the left, right, top and bottom planes into account.

            return true;
        }

        private void RenderChunk(TriangleBuffer buffer, Chunk chunk, int chunkX, int chunkY, int chunkZ)
        {
            float xOffset = (chunkX << Chunk.Bits) + 0.5f;
            float yOffset = (chunkY << Chunk.Bits) + 0.5f;
            float zOffset = (chunkZ << Chunk.Bits) + 0.5f;

            for (int y = Chunk.Size - 1; y >= 0; --y)
            {
                for (int x = 0; x < Chunk.Size; ++x)
                {
                    for (int z = 0; z < Chunk.Size; ++z)
                    {
                        ushort cube = chunk[x, y, z];
                        if (cube != 0)
                        {
                            var textureEntry = _textureAtlas.GetTextureEntry(cube);

                            float x1 = x + xOffset, y1 = y + yOffset, z1 = z + zOffset;

                            if (x == 0 || chunk[x - 1, y, z] == 0) DrawCubeLeft(buffer, textureEntry, x1, y1, z1, 0);
                            if (x == Chunk.Size - 1 || chunk[x + 1, y, z] == 0) DrawCubeRight(buffer, textureEntry, x1, y1, z1, 0);
                            if (y == 0 || chunk[x, y - 1, z] == 0) DrawCubeBottom(buffer, textureEntry, x1, y1, z1, 0);
                            if (y == Chunk.Size - 1 || chunk[x, y + 1, z] == 0) DrawCubeTop(buffer, textureEntry, x1, y1, z1, 0);
                            if (z == 0 || chunk[x, y, z - 1] == 0) DrawCubeBack(buffer, textureEntry, x1, y1, z1, 0);
                            if (z == Chunk.Size - 1 || chunk[x, y, z + 1] == 0) DrawCubeFront(buffer, textureEntry, x1, y1, z1, 0);
                        }
                    }
                }
            }
        }

        private void SetProjectionMatrix(RenderInfo renderInfo)
        {
            float dw = renderInfo.Width;
            float dh = renderInfo.Height;
            float dd = (float)Math.Sqrt(dw * dw + dh * dh);
            float df = 1.0f / (float)Math.Tan(_viewingAngle / 180.0 * Math.PI * 0.5);

            float yScale = df * dd / dh;
            float xScale = df * dd / dw;
            float near = 0.05f;
            float far = 1000.0f;
            float length = far - near;

            Matrix4 projectionMatrix = Matrix4.Zero;

            projectionMatrix.M11 = xScale;
            projectionMatrix.M22 = yScale;
            projectionMatrix.M33 = (-far - near) / length;
            projectionMatrix.M34 = -1;
            projectionMatrix.M43 = -2 * near * far / length;
            projectionMatrix.M44 = 0;

            _projectionMatrix = projectionMatrix;
        }

        private void DrawEntity(TriangleBuffer buffer, float x, float y, float z, float xRadius, float height)
        {
            var textureEntry = _textureAtlas.GetTextureEntry(0);

            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);
            buffer.VertexFloat(x - xRadius, y, z + xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexByte(217, 217, 217, 0); buffer.EndVertex();
            buffer.VertexFloat(x + xRadius, y, z + xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexByte(217, 217, 217, 0); buffer.EndVertex();
            buffer.VertexFloat(x + xRadius, y + height, z + xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexByte(217, 217, 217, 0); buffer.EndVertex();
            buffer.VertexFloat(x - xRadius, y + height, z + xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexByte(217, 217, 217, 0); buffer.EndVertex();

            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);
            buffer.VertexFloat(x + xRadius, y, z + xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x + xRadius, y, z - xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x + xRadius, y + height, z - xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x + xRadius, y + height, z + xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();

            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);
            buffer.VertexFloat(x + xRadius, y, z - xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x - xRadius, y, z - xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x - xRadius, y + height, z - xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x + xRadius, y + height, z - xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();

            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);
            buffer.VertexFloat(x - xRadius, y, z - xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x - xRadius, y, z + xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x - xRadius, y + height, z + xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x - xRadius, y + height, z - xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();

            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);
            buffer.VertexFloat(x - xRadius, y, z + xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x - xRadius, y, z - xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x + xRadius, y, z - xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();
            buffer.VertexFloat(x + xRadius, y, z + xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexByte(178, 204, 230, 0); buffer.EndVertex();

            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);
            buffer.VertexFloat(x - xRadius, y + height, z + xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexByte(255, 255, 255, 0); buffer.EndVertex();
            buffer.VertexFloat(x + xRadius, y + height, z + xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexByte(255, 255, 255, 0); buffer.EndVertex();
            buffer.VertexFloat(x + xRadius, y + height, z - xRadius); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexByte(255, 255, 255, 0); buffer.EndVertex();
            buffer.VertexFloat(x - xRadius, y + height, z - xRadius); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexByte(255, 255, 255, 0); buffer.EndVertex();
        }

        private void DrawCubeFront(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
        {
            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);

            buffer.VertexFloat(x - 0.5f, y - 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y - 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
        }

        private void DrawCubeRight(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
        {
            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);

            buffer.VertexFloat(x + 0.5f, y - 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y - 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
        }

        private void DrawCubeBack(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
        {
            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);

            buffer.VertexFloat(x + 0.5f, y - 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y - 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
        }

        private void DrawCubeLeft(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
        {
            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);

            buffer.VertexFloat(x - 0.5f, y - 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y - 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
        }

        private void DrawCubeTop(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
        {
            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);

            buffer.VertexFloat(x - 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexColorBytes(1f, 1f, 1f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexColorBytes(1f, 1f, 1f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexColorBytes(1f, 1f, 1f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexColorBytes(1f, 1f, 1f, highlight); buffer.EndVertex();
        }

        private void DrawCubeBottom(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
        {
            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);

            buffer.VertexFloat(x - 0.5f, y - 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexColorBytes(0.33f, 0.33f, 0.33f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y - 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexColorBytes(0.33f, 0.33f, 0.33f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y - 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexColorBytes(0.33f, 0.33f, 0.33f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y - 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexColorBytes(0.33f, 0.33f, 0.33f, highlight); buffer.EndVertex();
        }

        private class ChunkBufferEntry
        {
            public ulong ContentHash;

            public VertexArray VertexArray;

            public GameTime LastAccess;
        }
    }
}
