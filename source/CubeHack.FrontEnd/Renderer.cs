// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Game;
using CubeHack.Geometry;
using CubeHack.Util;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

using static CubeHack.Geometry.GeometryConstants;

namespace CubeHack.FrontEnd
{
    internal class Renderer
    {
        private const float _viewingAngle = 100f;
        private const int _maxChunkUpdatesPerFrame = 4;

        private readonly double ChunkRadius = Math.Sqrt(3 * ExtraMath.Square((double)Chunk.Size));

        private readonly Lazy<Shader> _cubeShader = new Lazy<Shader>(() => Shader.Load("CubeHack.FrontEnd.Shaders.Cube"));
        private readonly Lazy<Shader> _postProcessShader = new Lazy<Shader>(() => Shader.Load("CubeHack.FrontEnd.Shaders.PostProcess"));
        private readonly Lazy<int> _depthBufferTexture = new Lazy<int>(() => GL.GenTexture());

        private readonly Dictionary<ChunkPos, DisplayListEntry> _displayLists = new Dictionary<ChunkPos, DisplayListEntry>();

        private int _depthBufferWidth;
        private int _depthBufferHeight;

        private int[] _displayListsToRender = new int[16];

        public void Render(GameClient gameClient, int width, int height)
        {
            GL.ClearColor(0.5f, 0.6f, 0.9f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            SetProjectionMatrix(width, height);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Rotate(gameClient.PositionData.VAngle, 1, 0, 0);
            GL.Rotate(gameClient.PositionData.HAngle, 0, -1, 0);

            var offset = gameClient.PositionData.Position - new Position();
            GL.Translate(-offset.X, -offset.Y - gameClient.PhysicsValues.PlayerEyeHeight, -offset.Z);

            GL.Disable(EnableCap.Texture2D);

            if (gameClient.EntityPositions != null)
            {
                foreach (var position in gameClient.EntityPositions)
                {
                    GL.PushMatrix();
                    var offset2 = position.Position - new Position();
                    GL.Translate(offset2.X, offset2.Y, offset2.Z);
                    GL.Rotate(position.HAngle + 180, 0, 1, 0);
                    DrawEntity(0, 0, 0, 0.5f * gameClient.PhysicsValues.PlayerWidth, gameClient.PhysicsValues.PlayerHeight);
                    GL.PopMatrix();
                }
            }

            GL.Enable(EnableCap.Texture2D);
            GameApp.Instance.TextureAtlas.Bind();

            RenderCubes(gameClient);

            var highlightedCube = gameClient.HighlightedCube;
            if (highlightedCube != null)
            {
                GL.Enable(EnableCap.PolygonOffsetFill);
                GL.DepthMask(false);
                GL.PolygonOffset(-1f, -1f);
                GL.UseProgram(_cubeShader.Value.Id);
                GL.SecondaryColor3(0.2f, 0.2f, 0.2f);
                GL.Begin(PrimitiveType.Quads);
                var textureEntry = GameApp.Instance.TextureAtlas.GetTextureEntry(gameClient.World[highlightedCube.CubeX, highlightedCube.CubeY, highlightedCube.CubeZ] - 1);
                if (highlightedCube.NormalX < 0) DrawCubeLeft(textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f);
                if (highlightedCube.NormalX > 0) DrawCubeRight(textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f);
                if (highlightedCube.NormalY < 0) DrawCubeBottom(textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f);
                if (highlightedCube.NormalY > 0) DrawCubeTop(textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f);
                if (highlightedCube.NormalZ < 0) DrawCubeBack(textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f);
                if (highlightedCube.NormalZ > 0) DrawCubeFront(textureEntry, highlightedCube.CubeX + 0.5f, highlightedCube.CubeY + 0.5f, highlightedCube.CubeZ + 0.5f);
                GL.End();
                GL.SecondaryColor3(0f, 0f, 0f);
                GL.UseProgram(0);
                GL.DepthMask(true);
                GL.Disable(EnableCap.PolygonOffsetFill);
            }

            RenderOutlines(width, height);
        }

        private void RenderCubes(GameClient gameClient)
        {
            GL.UseProgram(_cubeShader.Value.Id);

            int chunkX = gameClient.PositionData.Position.CubeX >> Chunk.Bits;
            int chunkY = gameClient.PositionData.Position.CubeY >> Chunk.Bits;
            int chunkZ = gameClient.PositionData.Position.CubeZ >> Chunk.Bits;

            var offset = (gameClient.PositionData.Position - new Position()) + new Offset(0, gameClient.PhysicsValues.PlayerEyeHeight, 0);

            int displayListCount = 0;
            int chunkUpdates = 0;

            for (int y = chunkY + ChunkViewRadiusY; y >= chunkY - ChunkViewRadiusY; --y)
            {
                for (int x = chunkX - ChunkViewRadiusXZ; x <= chunkX + ChunkViewRadiusXZ; ++x)
                {
                    for (int z = chunkZ - ChunkViewRadiusXZ; z <= chunkZ + ChunkViewRadiusXZ; ++z)
                    {
                        var chunkPos = new ChunkPos(x, y, z);
                        if (!IsInViewingFrustum(gameClient, offset, x, y, z)) continue;

                        var chunk = gameClient.World.PeekChunk(chunkPos);
                        if (chunk != null && chunk.HasData)
                        {
                            DisplayListEntry entry;
                            _displayLists.TryGetValue(chunkPos, out entry);
                            int displayList = entry == null ? 0 : entry.DisplayList;

                            if ((displayList == 0 || entry.ContentHash != chunk.ContentHash) && chunkUpdates < _maxChunkUpdatesPerFrame)
                            {
                                ++chunkUpdates;

                                if (displayList == 0)
                                {
                                    displayList = GL.GenLists(1);
                                }

                                GL.NewList(displayList, ListMode.Compile);
                                RenderChunk(chunk, x, y, z);
                                GL.EndList();

                                _displayLists[chunkPos] = new DisplayListEntry { ContentHash = chunk.ContentHash, DisplayList = displayList };
                            }

                            if (displayList != 0)
                            {
                                if (displayListCount == _displayListsToRender.Length)
                                {
                                    var newArray = new int[_displayListsToRender.Length * 2];
                                    Array.Copy(_displayListsToRender, newArray, _displayListsToRender.Length);
                                    _displayListsToRender = newArray;
                                }

                                _displayListsToRender[displayListCount] = displayList;
                                ++displayListCount;
                            }
                        }
                    }
                }
            }

            var keysToRemove = new HashSet<ChunkPos>();
            foreach (var entry in _displayLists)
            {
                var chunkPos = entry.Key;
                if (chunkPos.X < chunkX - ChunkViewRadiusXZ || chunkPos.X > chunkX + ChunkViewRadiusXZ ||
                    chunkPos.Y < chunkY - ChunkViewRadiusY || chunkPos.Y > chunkY + ChunkViewRadiusY ||
                    chunkPos.Z < chunkZ - ChunkViewRadiusXZ || chunkPos.Z > chunkZ + ChunkViewRadiusXZ)
                {
                    GL.DeleteLists(entry.Value.DisplayList, 1);
                    keysToRemove.Add(chunkPos);
                }
            }

            foreach (var entry in keysToRemove)
            {
                _displayLists.Remove(entry);
            }

            if (displayListCount > 0)
            {
                GL.CallLists(displayListCount, ListNameType.Int, _displayListsToRender);
            }

            GL.UseProgram(0);
        }

        private bool IsInViewingFrustum(GameClient gameClient, Offset offset, int x, int y, int z)
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

        private void RenderChunk(Chunk chunk, int chunkX, int chunkY, int chunkZ)
        {
            float xOffset = (chunkX << Chunk.Bits) + 0.5f;
            float yOffset = (chunkY << Chunk.Bits) + 0.5f;
            float zOffset = (chunkZ << Chunk.Bits) + 0.5f;

            GL.Begin(PrimitiveType.Quads);
            for (int x = 0; x < Chunk.Size; ++x)
            {
                for (int y = 0; y < Chunk.Size; ++y)
                {
                    for (int z = 0; z < Chunk.Size; ++z)
                    {
                        ushort cube = chunk[x, y, z];
                        if (cube != 0)
                        {
                            var textureEntry = GameApp.Instance.TextureAtlas.GetTextureEntry((int)cube - 1);

                            float x1 = x + xOffset, y1 = y + yOffset, z1 = z + zOffset;

                            if (x == 0 || chunk[x - 1, y, z] == 0) DrawCubeLeft(textureEntry, x1, y1, z1);
                            if (x == Chunk.Size - 1 || chunk[x + 1, y, z] == 0) DrawCubeRight(textureEntry, x1, y1, z1);
                            if (y == 0 || chunk[x, y - 1, z] == 0) DrawCubeBottom(textureEntry, x1, y1, z1);
                            if (y == Chunk.Size - 1 || chunk[x, y + 1, z] == 0) DrawCubeTop(textureEntry, x1, y1, z1);
                            if (z == 0 || chunk[x, y, z - 1] == 0) DrawCubeBack(textureEntry, x1, y1, z1);
                            if (z == Chunk.Size - 1 || chunk[x, y, z + 1] == 0) DrawCubeFront(textureEntry, x1, y1, z1);
                        }
                    }
                }
            }
            GL.End();
        }

        private void RenderOutlines(int width, int height)
        {
            GL.Flush();
            GL.UseProgram(_postProcessShader.Value.Id);

            GL.BindTexture(TextureTarget.Texture2D, _depthBufferTexture.Value);

            if (width != _depthBufferWidth || height != _depthBufferHeight)
            {
                _depthBufferWidth = width;
                _depthBufferHeight = height;
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            }

            GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, width, height);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0, 0);
            GL.Vertex3(-1, -1, 0);
            GL.TexCoord2(1, 0);
            GL.Vertex3(1, -1, 0);
            GL.TexCoord2(1, 1);
            GL.Vertex3(1, 1, 0);
            GL.TexCoord2(0, 1);
            GL.Vertex3(-1, 1, 0);
            GL.End();

            GL.UseProgram(0);
        }

        private void SetProjectionMatrix(float width, float height)
        {
            float dw = width;
            float dh = height;
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

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projectionMatrix);
        }

        private void DrawEntity(float x, float y, float z, float xRadius, float height)
        {
            GL.Begin(PrimitiveType.Quads);

            GL.Color3(0.85f, 0.85f, 0.85f);
            GL.Vertex3(x - xRadius, y, z + xRadius);
            GL.Vertex3(x + xRadius, y, z + xRadius);
            GL.Vertex3(x + xRadius, y + height, z + xRadius);
            GL.Vertex3(x - xRadius, y + height, z + xRadius);

            GL.Color3(0.7f, 0.8f, 0.9f);

            GL.Vertex3(x + xRadius, y, z + xRadius);
            GL.Vertex3(x + xRadius, y, z - xRadius);
            GL.Vertex3(x + xRadius, y + height, z - xRadius);
            GL.Vertex3(x + xRadius, y + height, z + xRadius);

            GL.Vertex3(x + xRadius, y, z - xRadius);
            GL.Vertex3(x - xRadius, y, z - xRadius);
            GL.Vertex3(x - xRadius, y + height, z - xRadius);
            GL.Vertex3(x + xRadius, y + height, z - xRadius);

            GL.Vertex3(x - xRadius, y, z - xRadius);
            GL.Vertex3(x - xRadius, y, z + xRadius);
            GL.Vertex3(x - xRadius, y + height, z + xRadius);
            GL.Vertex3(x - xRadius, y + height, z - xRadius);

            GL.Color3(0.35f, 0.4f, 0.45f);

            GL.Vertex3(x - xRadius, y, z + xRadius);
            GL.Vertex3(x - xRadius, y, z - xRadius);
            GL.Vertex3(x + xRadius, y, z - xRadius);
            GL.Vertex3(x + xRadius, y, z + xRadius);

            GL.Color3(1.0f, 1.0f, 1.0f);

            GL.Vertex3(x - xRadius, y + height, z + xRadius);
            GL.Vertex3(x + xRadius, y + height, z + xRadius);
            GL.Vertex3(x + xRadius, y + height, z - xRadius);
            GL.Vertex3(x - xRadius, y + height, z - xRadius);

            GL.End();
        }

        private void DrawCubeFront(TextureAtlas.TextureEntry textureEntry, float x, float y, float z)
        {
            GL.Color3(0.67f, 0.67f, 0.67f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.5f, y - 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.5f, y - 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.5f, y + 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.5f, y + 0.5f, z + 0.5f);
        }

        private void DrawCubeRight(TextureAtlas.TextureEntry textureEntry, float x, float y, float z)
        {
            GL.Color3(0.67f, 0.67f, 0.67f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x + 0.5f, y - 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.5f, y + 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x + 0.5f, y + 0.5f, z + 0.5f);
        }

        private void DrawCubeBack(TextureAtlas.TextureEntry textureEntry, float x, float y, float z)
        {
            GL.Color3(0.67f, 0.67f, 0.67f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x + 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x - 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x - 0.5f, y + 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x + 0.5f, y + 0.5f, z - 0.5f);
        }

        private void DrawCubeLeft(TextureAtlas.TextureEntry textureEntry, float x, float y, float z)
        {
            GL.Color3(0.67f, 0.67f, 0.67f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x - 0.5f, y - 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x - 0.5f, y + 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.5f, y + 0.5f, z - 0.5f);
        }

        private void DrawCubeTop(TextureAtlas.TextureEntry textureEntry, float x, float y, float z)
        {
            GL.Color3(1f, 1f, 1f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.5f, y + 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.5f, y + 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.5f, y + 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.5f, y + 0.5f, z - 0.5f);
        }

        private void DrawCubeBottom(TextureAtlas.TextureEntry textureEntry, float x, float y, float z)
        {
            GL.Color3(0.33f, 0.33f, 0.33f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.5f, y - 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.5f, y - 0.5f, z + 0.5f);
        }

        private void SetHighlightColor(RayCastResult highlightedCube, int normalX, int normalY, int normalZ)
        {
            if (highlightedCube != null
                && highlightedCube.NormalX == normalX
                && highlightedCube.NormalY == normalY
                && highlightedCube.NormalZ == normalZ)
            {
                GL.SecondaryColor3(0.2f, 0.2f, 0.2f);
            }
            else
            {
                GL.SecondaryColor3(0f, 0f, 0f);
            }
        }

        private class DisplayListEntry
        {
            public ulong ContentHash;

            public int DisplayList;
        }
    }
}
