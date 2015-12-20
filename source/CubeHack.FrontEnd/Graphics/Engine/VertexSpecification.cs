// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace CubeHack.FrontEnd.Graphics.Engine
{
    internal sealed class VertexSpecification : System.Collections.IEnumerable
    {
        private readonly List<Entry> _entries;

        private int _vertexSize;
        private int _vertexStride;

        public VertexSpecification()
        {
            _entries = new List<Entry>();
        }

        public VertexSpecification(VertexSpecification specification)
        {
            _entries = new List<Entry>(specification._entries);
        }

        public int Stride => _vertexStride;

        public int EntryCount => _entries.Count;

        public Entry this[int index] => _entries[index];

        public void Add(int location, int count, VertexAttribPointerType type, bool isNormalized)
        {
            if (count < 1 || count > 4) throw new ArgumentOutOfRangeException(nameof(count));

            int entryAlignment = GetAlignment(type);
            int entryOffset = Align(_vertexSize, entryAlignment);
            _entries.Add(new Entry(location, count, type, isNormalized, entryAlignment, entryOffset));

            int singleSize = GetSize(type);
            int entrySize = (count - 1) * Align(singleSize, entryAlignment) + singleSize;
            _vertexSize = entryOffset + entrySize;
            _vertexStride = Align(_vertexSize, _entries[0].Alignment);
        }

        public void Bind()
        {
            for (var i = 0; i < _entries.Count; ++i)
            {
                var entry = _entries[i];
                GL.EnableVertexAttribArray(entry.Location);
                GL.VertexAttribPointer(entry.Location, entry.Count, entry.Type, entry.IsNormalized, _vertexStride, entry.Offset);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private static int Align(int index, int alignment)
        {
            return (index + alignment - 1) & ~(alignment - 1);
        }

        private static int GetAlignment(VertexAttribPointerType type)
        {
            switch (type)
            {
                case VertexAttribPointerType.Byte: return 1;
                case VertexAttribPointerType.Float: return 4;
                case VertexAttribPointerType.Int: return 4;
                case VertexAttribPointerType.Short: return 2;
                case VertexAttribPointerType.UnsignedByte: return 1;
                case VertexAttribPointerType.UnsignedInt: return 4;
                case VertexAttribPointerType.UnsignedShort: return 2;
                default: throw new NotImplementedException($"VertexSpecification.GetAlignment not implemented for 'VertexAttribPointerType.{type}'");
            }
        }

        private static int GetSize(VertexAttribPointerType type)
        {
            switch (type)
            {
                case VertexAttribPointerType.Byte: return 1;
                case VertexAttribPointerType.Float: return 4;
                case VertexAttribPointerType.Int: return 4;
                case VertexAttribPointerType.Short: return 2;
                case VertexAttribPointerType.UnsignedByte: return 1;
                case VertexAttribPointerType.UnsignedInt: return 4;
                case VertexAttribPointerType.UnsignedShort: return 2;
                default: throw new NotImplementedException($"VertexSpecification.GetSize not implemented for 'VertexAttribPointerType.{type}'");
            }
        }

        public class Entry
        {
            public Entry(int location, int count, VertexAttribPointerType type, bool isNormalized, int alignment, int offset)
            {
                Location = location;
                Count = count;
                Type = type;
                IsNormalized = isNormalized;
                Alignment = alignment;
                Offset = offset;
            }

            public int Location { get; }

            public int Count { get; }

            public VertexAttribPointerType Type { get; }

            public bool IsNormalized { get; }

            public int Alignment { get; }

            public int Offset { get; }
        }
    }
}
