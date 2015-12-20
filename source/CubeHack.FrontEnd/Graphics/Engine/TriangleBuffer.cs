// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using OpenTK.Graphics.OpenGL4;
using System;
using System.Diagnostics;

namespace CubeHack.FrontEnd.Graphics.Engine
{
    internal sealed class TriangleBuffer : IDisposable
    {
        private readonly ByteBuffer _indices = new ByteBuffer();
        private readonly ByteBuffer _vertices = new ByteBuffer();

        private int _indexCount;
        private uint _vertexCount;

        private int _specificationIndex;

        public TriangleBuffer(VertexSpecification specification)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            Specification = specification;
        }

        public int IndexCount => _indexCount;

        private VertexSpecification Specification { get; }

        public void Clear()
        {
            _vertices.Clear();
            _indices.Clear();
            _indexCount = 0;
            _vertexCount = 0;
            _specificationIndex = 0;
        }

        public void Dispose()
        {
            _vertices.Dispose();
            _indices.Dispose();
            _indexCount = 0;
            _vertexCount = 0;
            _specificationIndex = 0;
        }

        public void UploadData(BufferUsageHint usageHint)
        {
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)_indices.ByteCount, _indices.Ptr, usageHint);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)_vertices.ByteCount, _vertices.Ptr, usageHint);
        }

        public void Triangle(uint i0, uint i1, uint i2)
        {
            unsafe
            {
                var data = (uint*)_indices.Allocate4(3);
                data[0] = _vertexCount + i0;
                data[1] = _vertexCount + i1;
                data[2] = _vertexCount + i2;
            }

            _indexCount += 3;
        }

        public void EndVertex()
        {
            ++_vertexCount;
            CheckEndVertex();
        }

        public void VertexFloat(float v0)
        {
            CheckAttribute(1, VertexAttribPointerType.Float);
            unsafe
            {
                var data = (float*)_vertices.Allocate4(1);
                data[0] = v0;
            }
        }

        public void VertexFloat(float v0, float v1)
        {
            CheckAttribute(2, VertexAttribPointerType.Float);
            unsafe
            {
                var data = (float*)_vertices.Allocate4(2);
                data[0] = v0;
                data[1] = v1;
            }
        }

        public void VertexFloat(float v0, float v1, float v2)
        {
            CheckAttribute(3, VertexAttribPointerType.Float);
            unsafe
            {
                var data = (float*)_vertices.Allocate4(3);
                data[0] = v0;
                data[1] = v1;
                data[2] = v2;
            }
        }

        public void VertexFloat(float v0, float v1, float v2, float v3)
        {
            CheckAttribute(4, VertexAttribPointerType.Float);
            unsafe
            {
                var data = (float*)_vertices.Allocate4(4);
                data[0] = v0;
                data[1] = v1;
                data[2] = v2;
                data[3] = v3;
            }
        }

        public void VertexByte(byte v0)
        {
            CheckAttribute(1, VertexAttribPointerType.UnsignedByte);
            unsafe
            {
                var data = (byte*)_vertices.Allocate1(1);
                data[0] = v0;
            }
        }

        public void VertexByte(byte v0, byte v1)
        {
            CheckAttribute(2, VertexAttribPointerType.UnsignedByte);
            unsafe
            {
                var data = (byte*)_vertices.Allocate1(2);
                data[0] = v0;
                data[1] = v1;
            }
        }

        public void VertexByte(byte v0, byte v1, byte v2)
        {
            CheckAttribute(3, VertexAttribPointerType.UnsignedByte);
            unsafe
            {
                var data = (byte*)_vertices.Allocate1(3);
                data[0] = v0;
                data[1] = v1;
                data[2] = v2;
            }
        }

        public void VertexByte(byte v0, byte v1, byte v2, byte v3)
        {
            CheckAttribute(4, VertexAttribPointerType.UnsignedByte);
            unsafe
            {
                var data = (byte*)_vertices.Allocate1(4);
                data[0] = v0;
                data[1] = v1;
                data[2] = v2;
                data[3] = v3;
            }
        }

        public void VertexUInt16(ushort v0)
        {
            CheckAttribute(1, VertexAttribPointerType.UnsignedShort);
            unsafe
            {
                var data = (ushort*)_vertices.Allocate2(1);
                data[0] = v0;
            }
        }

        public void VertexUInt16(ushort v0, ushort v1)
        {
            CheckAttribute(2, VertexAttribPointerType.UnsignedShort);
            unsafe
            {
                var data = (ushort*)_vertices.Allocate2(2);
                data[0] = v0;
                data[1] = v1;
            }
        }

        public void VertexUInt16(ushort v0, ushort v1, ushort v2)
        {
            CheckAttribute(3, VertexAttribPointerType.UnsignedShort);
            unsafe
            {
                var data = (ushort*)_vertices.Allocate2(3);
                data[0] = v0;
                data[1] = v1;
                data[2] = v2;
            }
        }

        public void VertexUInt16(ushort v0, ushort v1, ushort v2, ushort v3)
        {
            CheckAttribute(4, VertexAttribPointerType.UnsignedShort);
            unsafe
            {
                var data = (ushort*)_vertices.Allocate2(4);
                data[0] = v0;
                data[1] = v1;
                data[2] = v2;
                data[3] = v3;
            }
        }

        public void VertexUInt32(uint v0)
        {
            CheckAttribute(1, VertexAttribPointerType.UnsignedInt);
            unsafe
            {
                var data = (uint*)_vertices.Allocate4(1);
                data[0] = v0;
            }
        }

        public void VertexUInt32(uint v0, uint v1)
        {
            CheckAttribute(2, VertexAttribPointerType.UnsignedInt);
            unsafe
            {
                var data = (uint*)_vertices.Allocate4(2);
                data[0] = v0;
                data[1] = v1;
            }
        }

        public void VertexUInt32(uint v0, uint v1, uint v2)
        {
            CheckAttribute(3, VertexAttribPointerType.UnsignedInt);
            unsafe
            {
                var data = (uint*)_vertices.Allocate4(3);
                data[0] = v0;
                data[1] = v1;
                data[2] = v2;
            }
        }

        public void VertexUInt32(uint v0, uint v1, uint v2, uint v3)
        {
            CheckAttribute(4, VertexAttribPointerType.UnsignedInt);
            unsafe
            {
                var data = (uint*)_vertices.Allocate4(4);
                data[0] = v0;
                data[1] = v1;
                data[2] = v2;
                data[3] = v3;
            }
        }

        [Conditional("DEBUG")]
        private void CheckEndVertex()
        {
            if (_specificationIndex != Specification.EntryCount) throw new InvalidOperationException("Incomplete vertex");
            _specificationIndex = 0;
        }

        [Conditional("DEBUG")]
        private void CheckAttribute(int count, VertexAttribPointerType type)
        {
            var entry = Specification[_specificationIndex];
            if (entry.Count != count || entry.Type != type) throw new InvalidOperationException("Invalid vertex attribute type");
            ++_specificationIndex;
        }
    }
}
