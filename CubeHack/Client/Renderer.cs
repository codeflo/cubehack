// Copyright (c) 2014 the CubeHack authors. All rights reserved.
// Licensed under a BSD 2-clause license, see LICENSE.txt for details.

using CubeHack.Game;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeHack.Client
{
    static class Renderer
    {
        const float _viewingAngle = 100f;

        static readonly Lazy<Shader> _postProcessShader = new Lazy<Shader>(() => Shader.Load("CubeHack.Client.Shaders.PostProcess"));
        static readonly Lazy<int> _depthBufferTexture = new Lazy<int>(() => GL.GenTexture());
        static int _depthBufferWidth;
        static int _depthBufferHeight;

        public static void Render(GameClient gameClient, int width, int height)
        {
            GL.Viewport(0, 0, width, height);
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
                float dt = gameClient.TimeSinceGameEvent;

                foreach (var position in gameClient.EntityPositions)
                {
                    GL.PushMatrix();
                    var offset2 = position.Position + dt * position.Velocity - new Position();
                    GL.Translate(offset2.X, offset2.Y, offset2.Z);
                    GL.Rotate(position.HAngle + 180, 0, 1, 0);
                    DrawEntity(0, 0, 0, 0.5f * gameClient.PhysicsValues.PlayerWidth, gameClient.PhysicsValues.PlayerHeight);
                    GL.PopMatrix();
                }
            }

            GL.Enable(EnableCap.Texture2D);
            TextureAtlas.Bind();

            var chunkData = gameClient.World.ChunkData;
            if (chunkData != null)
            {
                for (int x = chunkData.X0; x < chunkData.X1; ++x)
                {
                    for (int y = chunkData.Y0; y < chunkData.Y1; ++y)
                    {
                        for (int z = chunkData.Z0; z < chunkData.Z1; ++z)
                        {
                            if (chunkData[x, y, z] != 0)
                            {
                                DrawCube(x, y, z);
                            }
                        }
                    }
                }
            }

            RenderOutlines(width, height);
        }

        private static void RenderOutlines(int width, int height)
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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

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

        static void SetProjectionMatrix(float width, float height)
        {
            float dw = width;
            float dh = height;
            float dd = (float)Math.Sqrt(dw * dw + dh * dh);
            float df = 1.0f / (float)Math.Tan(_viewingAngle / 180.0 * Math.PI * 0.5);

            float yScale = df * dd / dh;
            float xScale = df * dd / dw;
            float near = 0.1f;
            float far = 100.0f;
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

        static void DrawEntity(float x, float y, float z, float xRadius, float height)
        {
            GL.Begin(PrimitiveType.Quads);

            GL.Color3(0.9f, 0.9f, 0.9f);
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

            GL.End();
        }

        static void DrawCube(float x, float y, float z)
        {
            var textureEntry = TextureAtlas.GetTextureEntry(0);

            GL.Begin(PrimitiveType.Quads);

            GL.Color3(0.67f, 0.67f, 0.67f);

            x += 0.5f;
            y += 0.5f;
            z += 0.5f;

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.5f, y - 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.5f, y - 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.5f, y + 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.5f, y + 0.5f, z + 0.5f);

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x + 0.5f, y - 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.5f, y + 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x + 0.5f, y + 0.5f, z + 0.5f);

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x + 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x - 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x - 0.5f, y + 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x + 0.5f, y + 0.5f, z - 0.5f);

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x - 0.5f, y - 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x - 0.5f, y + 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.5f, y + 0.5f, z - 0.5f);

            GL.Color3(1f, 1f, 1f);

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.5f, y + 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.5f, y + 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.5f, y + 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.5f, y + 0.5f, z - 0.5f);

            GL.Color3(0.33f, 0.33f, 0.33f);

            GL.TexCoord2(textureEntry.X0, textureEntry.Y0); GL.Vertex3(x - 0.5f, y - 0.5f, z + 0.5f);
            GL.TexCoord2(textureEntry.X0, textureEntry.Y1); GL.Vertex3(x - 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y1); GL.Vertex3(x + 0.5f, y - 0.5f, z - 0.5f);
            GL.TexCoord2(textureEntry.X1, textureEntry.Y0); GL.Vertex3(x + 0.5f, y - 0.5f, z + 0.5f);

            GL.End();
        }
    }
}
