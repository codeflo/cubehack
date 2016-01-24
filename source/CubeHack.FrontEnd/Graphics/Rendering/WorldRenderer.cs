// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Data;
using CubeHack.FrontEnd.Graphics.Engine;
using CubeHack.Game;
using CubeHack.Geometry;
using CubeHack.State;
using CubeHack.Util;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CubeHack.Geometry.GeometryConstants;

namespace CubeHack.FrontEnd.Graphics.Rendering
{
    internal sealed class WorldRenderer : IDisposable
    {
        private const float _viewingAngle = 100f;

        private static readonly double ChunkRadius = Math.Sqrt(3 * ExtraMath.Square((double)GeometryConstants.ChunkSize));
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

        [DependencyInjected]
        public WorldRenderer(WorldTextureAtlas textureAtlas, OutlineRenderer outlineRenderer)
        {
            _textureAtlas = textureAtlas;
            _outlineRenderer = outlineRenderer;
        }

        public void Dispose()
        {
            _tempTriangles.Dispose();
        }

        public void Render(GameClient gameClient, RenderInfo renderInfo)
        {
            _currentFrameTime = GameTime.Now();

            GL.ClearColor(0.5f, 0.6f, 0.9f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            var offset = gameClient.PositionData.Placement.Pos - EntityPos.Origin;
            SetProjectionMatrix(renderInfo);

            _modelViewMatrix = Matrix4.Identity
                * Matrix4.CreateTranslation((float)-offset.X, (float)-offset.Y - gameClient.PhysicsValues.PlayerEyeHeight, (float)-offset.Z)
                * Matrix4.CreateRotationY((float)-gameClient.PositionData.Placement.Orientation.Horizontal)
                * Matrix4.CreateRotationX((float)gameClient.PositionData.Placement.Orientation.Vertical);

            _textureAtlas.Bind();

            RenderHighlightedFace(gameClient);

            RenderBlocks(gameClient);

            RenderEntities(gameClient);

            _outlineRenderer.RenderOutlines(renderInfo);
        }

        private void RenderHighlightedFace(GameClient gameClient)
        {
            var highlightedBlock = gameClient.HighlightedBlock;
            if (highlightedBlock == null) return;

            var textureEntry = _textureAtlas.GetTextureEntry(gameClient.World[highlightedBlock.BlockPos]);

            _tempTriangles.Clear();

            float highlight = 0.2f;
            if (highlightedBlock.Normal.X < 0) DrawBlockLeft(_tempTriangles, textureEntry, highlightedBlock.BlockPos.X + 0.5f, highlightedBlock.BlockPos.Y + 0.5f, highlightedBlock.BlockPos.Z + 0.5f, highlight);
            if (highlightedBlock.Normal.X > 0) DrawBlockRight(_tempTriangles, textureEntry, highlightedBlock.BlockPos.X + 0.5f, highlightedBlock.BlockPos.Y + 0.5f, highlightedBlock.BlockPos.Z + 0.5f, highlight);
            if (highlightedBlock.Normal.Y < 0) DrawBlockBottom(_tempTriangles, textureEntry, highlightedBlock.BlockPos.X + 0.5f, highlightedBlock.BlockPos.Y + 0.5f, highlightedBlock.BlockPos.Z + 0.5f, highlight);
            if (highlightedBlock.Normal.Y > 0) DrawBlockTop(_tempTriangles, textureEntry, highlightedBlock.BlockPos.X + 0.5f, highlightedBlock.BlockPos.Y + 0.5f, highlightedBlock.BlockPos.Z + 0.5f, highlight);
            if (highlightedBlock.Normal.Z < 0) DrawBlockBack(_tempTriangles, textureEntry, highlightedBlock.BlockPos.X + 0.5f, highlightedBlock.BlockPos.Y + 0.5f, highlightedBlock.BlockPos.Z + 0.5f, highlight);
            if (highlightedBlock.Normal.Z > 0) DrawBlockFront(_tempTriangles, textureEntry, highlightedBlock.BlockPos.X + 0.5f, highlightedBlock.BlockPos.Y + 0.5f, highlightedBlock.BlockPos.Z + 0.5f, highlight);

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
            var infos = gameClient.EntityInfos;
            if (infos == null) return;

            GL.UseProgram(_cubeShader.Value.Id);

            GL.UniformMatrix4(0, false, ref _projectionMatrix);
            GL.UniformMatrix4(4, false, ref _modelViewMatrix);

            foreach (var info in infos)
            {
                var offset = info.PositionData.Placement.Pos - EntityPos.Origin;

                float modelWidth = 0.5f * gameClient.PhysicsValues.PlayerWidth;
                float modelHeight = gameClient.PhysicsValues.PlayerHeight;

                if (info.ModelIndex != null)
                {
                    Model model = gameClient.ModData.Models[(int)info.ModelIndex];

                    modelWidth = model.Width;
                    modelHeight = model.Height;
                }

                GL.Uniform3(12, (float)offset.X, (float)offset.Y, (float)offset.Z);

                using (var vertexArray = new VertexArray(_cubeVertexSpecification))
                {
                    _tempTriangles.Clear();
                    DrawEntity(_tempTriangles, 0, 0, 0, modelWidth, modelHeight);
                    vertexArray.SetData(_tempTriangles, BufferUsageHint.DynamicDraw);
                    vertexArray.Draw();
                }

                GL.Uniform3(12, 0f, 0f, 0f);
            }

            GL.UseProgram(0);
        }

        private void RenderBlocks(GameClient gameClient)
        {
            GL.UseProgram(_cubeShader.Value.Id);

            GL.UniformMatrix4(0, false, ref _projectionMatrix);
            GL.UniformMatrix4(4, false, ref _modelViewMatrix);

            var cameraChunkPos = (ChunkPos)gameClient.PositionData.Placement.Pos;

            var offset = (gameClient.PositionData.Placement.Pos - EntityPos.Origin) + new EntityOffset(0, gameClient.PhysicsValues.PlayerEyeHeight, 0);

            int chunkUpdates = 0;

            ChunkPos.IterateOutwards(
                cameraChunkPos,
                ChunkViewRadiusXZ,
                ChunkViewRadiusY,
                chunkPos =>
                {
                    ChunkBufferEntry entry;
                    _chunkBuffers.TryGetValue(chunkPos, out entry);

                    /* Don't let nearby entries expire. */
                    if (entry != null) entry.LastAccess = _currentFrameTime;

                    if (!IsInViewingFrustum(gameClient, offset, chunkPos)) return;

                    var chunk = gameClient.World.PeekChunk(chunkPos);
                    if (chunk != null && chunk.HasData)
                    {
                        if (entry == null)
                        {
                            entry = new ChunkBufferEntry { LastAccess = _currentFrameTime };
                            _chunkBuffers[chunkPos] = entry;
                        }

                        if (entry.ContentHash != chunk.ContentHash)
                        {
                            if (chunkUpdates < 5 && entry.TriangleTask != null && entry.TriangleTask.IsCompleted)
                            {
                                var triangles = entry.TriangleTask.Result;

                                if (entry.TriangleTaskContentHash == chunk.ContentHash)
                                {
                                    ++chunkUpdates;
                                    if (entry.VertexArray == null) entry.VertexArray = new VertexArray(_cubeVertexSpecification);
                                    entry.VertexArray.SetData(triangles, BufferUsageHint.StaticDraw);
                                    entry.ContentHash = chunk.ContentHash;
                                }

                                triangles.Dispose();
                                entry.TriangleTask = null;
                                entry.TriangleTaskContentHash = 0;
                            }

                            if (entry.ContentHash != chunk.ContentHash && entry.TriangleTask == null)
                            {
                                var triangleBuffer = new TriangleBuffer(_cubeVertexSpecification);

                                var localChunk = chunk;
                                entry.TriangleTask = Task.Run(() => { RenderChunk(triangleBuffer, localChunk); return triangleBuffer; });
                                entry.TriangleTaskContentHash = chunk.ContentHash;
                            }
                        }

                        entry.VertexArray?.Draw();
                    }
                });

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
                    entry.Value.VertexArray?.Dispose();

                    /* If the task has completed, dispose the triangles here. If not, they are garbage collected anyway. */
                    if (entry.Value.TriangleTask != null && entry.Value.TriangleTask.IsCompleted) entry.Value.TriangleTask.Result.Dispose();

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

        private bool IsInViewingFrustum(GameClient gameClient, EntityOffset offset, ChunkPos chunkPos)
        {
            /* Check if the bounding sphere of the chunk is in the viewing frustum. */

            var df = (double)GeometryConstants.ChunkSize;

            // Determine the chunk center relative to the viewer.
            double dx = (chunkPos.X + 0.5) * df - offset.X;
            double dy = (chunkPos.Y + 0.5) * df - offset.Y;
            double dz = (chunkPos.Z + 0.5) * df - offset.Z;

            double t0, t1;

            // Perform mouselook rotation
            double ha = gameClient.PositionData.Placement.Orientation.Horizontal;
            double hc = Math.Cos(ha), hs = Math.Sin(ha);
            t0 = dx * hc - dz * hs; t1 = dz * hc + dx * hs; dx = t0; dz = t1;

            double va = -gameClient.PositionData.Placement.Orientation.Vertical;
            double vc = Math.Cos(va), vs = Math.Sin(va);
            t0 = dz * vc - dy * vs; t1 = dy * vc + dz * vs; dz = t0; dy = t1;

            // Check if the chunk is behind the viewer.
            if (dz > ChunkRadius && dz > ChunkRadius) return false;

            /* TODO: We can discard even more chunks by taking the left, right, top and bottom planes into account. */

            return true;
        }

        private void RenderChunk(TriangleBuffer buffer, Chunk chunk)
        {
            var offset = chunk.Pos - new BlockPos(0, 0, 0);
            float xOffset = offset.X + 0.5f;
            float yOffset = offset.Y + 0.5f;
            float zOffset = offset.Z + 0.5f;

            for (int y = GeometryConstants.ChunkSize - 1; y >= 0; --y)
            {
                for (int x = 0; x < GeometryConstants.ChunkSize; ++x)
                {
                    for (int z = 0; z < GeometryConstants.ChunkSize; ++z)
                    {
                        ushort material = chunk[x, y, z];
                        if (material != 0)
                        {
                            var textureEntry = _textureAtlas.GetTextureEntry(material);

                            float x1 = x + xOffset, y1 = y + yOffset, z1 = z + zOffset;

                            if (x == 0 || chunk[x - 1, y, z] == 0) DrawBlockLeft(buffer, textureEntry, x1, y1, z1, 0);
                            if (x == GeometryConstants.ChunkSize - 1 || chunk[x + 1, y, z] == 0) DrawBlockRight(buffer, textureEntry, x1, y1, z1, 0);
                            if (y == 0 || chunk[x, y - 1, z] == 0) DrawBlockBottom(buffer, textureEntry, x1, y1, z1, 0);
                            if (y == GeometryConstants.ChunkSize - 1 || chunk[x, y + 1, z] == 0) DrawBlockTop(buffer, textureEntry, x1, y1, z1, 0);
                            if (z == 0 || chunk[x, y, z - 1] == 0) DrawBlockBack(buffer, textureEntry, x1, y1, z1, 0);
                            if (z == GeometryConstants.ChunkSize - 1 || chunk[x, y, z + 1] == 0) DrawBlockFront(buffer, textureEntry, x1, y1, z1, 0);
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

        private void DrawBlockFront(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
        {
            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);

            buffer.VertexFloat(x - 0.5f, y - 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y - 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
        }

        private void DrawBlockRight(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
        {
            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);

            buffer.VertexFloat(x + 0.5f, y - 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y - 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
        }

        private void DrawBlockBack(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
        {
            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);

            buffer.VertexFloat(x + 0.5f, y - 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y - 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
        }

        private void DrawBlockLeft(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
        {
            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);

            buffer.VertexFloat(x - 0.5f, y - 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y - 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexColorBytes(0.67f, 0.67f, 0.67f, highlight); buffer.EndVertex();
        }

        private void DrawBlockTop(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
        {
            buffer.Triangle(0, 1, 3);
            buffer.Triangle(3, 1, 2);

            buffer.VertexFloat(x - 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y0); buffer.VertexColorBytes(1f, 1f, 1f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z + 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y0); buffer.VertexColorBytes(1f, 1f, 1f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x + 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X1, textureEntry.Y1); buffer.VertexColorBytes(1f, 1f, 1f, highlight); buffer.EndVertex();
            buffer.VertexFloat(x - 0.5f, y + 0.5f, z - 0.5f); buffer.VertexUInt16(textureEntry.X0, textureEntry.Y1); buffer.VertexColorBytes(1f, 1f, 1f, highlight); buffer.EndVertex();
        }

        private void DrawBlockBottom(TriangleBuffer buffer, WorldTextureAtlas.TextureEntry textureEntry, float x, float y, float z, float highlight)
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

            public ulong TriangleTaskContentHash;
            public Task<TriangleBuffer> TriangleTask;

            public GameTime LastAccess;
        }
    }
}
