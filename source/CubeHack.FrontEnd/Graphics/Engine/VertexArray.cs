// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using OpenTK.Graphics.OpenGL4;
using System;

namespace CubeHack.FrontEnd.Graphics.Engine
{
    internal sealed class VertexArray : IDisposable
    {
        private readonly VertexSpecification _specification;

        private int _vertexArray;
        private int _elementArrayBuffer;
        private int _arrayBuffer;

        private int _indexCount = 0;

        public VertexArray(VertexSpecification specification)
        {
            _specification = specification;
        }

        public void SetData(TriangleBuffer triangles, BufferUsageHint usageHint)
        {
            _indexCount = triangles.IndexCount;
            if (_indexCount % 3 != 0) throw new IndexOutOfRangeException("Number of index entries must be divisible by 3.");

            if (_indexCount == 0)
            {
                Dispose();
                return;
            }

            if (_vertexArray == 0)
            {
                _vertexArray = GL.GenVertexArray();
                _elementArrayBuffer = GL.GenBuffer();
                _arrayBuffer = GL.GenBuffer();

                GL.BindVertexArray(_vertexArray);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementArrayBuffer);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _arrayBuffer);
                _specification.Bind();
            }
            else
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementArrayBuffer);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _arrayBuffer);
            }

            triangles.UploadData(usageHint);
        }

        public void Draw()
        {
            if (_vertexArray == 0 || _indexCount == 0) return;
            GL.BindVertexArray(_vertexArray);
            GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public void Dispose()
        {
            if (_vertexArray != 0)
            {
                GL.DeleteBuffer(_arrayBuffer);
                _arrayBuffer = 0;
                GL.DeleteBuffer(_elementArrayBuffer);
                _elementArrayBuffer = 0;
                GL.DeleteVertexArray(_vertexArray);
                _vertexArray = 0;
            }

            _indexCount = 0;
        }
    }
}
