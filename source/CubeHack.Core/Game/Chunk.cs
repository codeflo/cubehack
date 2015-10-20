// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Geometry;
using System;
using System.IO;

namespace CubeHack.Game
{
    public class Chunk
    {
        public const int Bits = 5;
        public const int Size = 1 << Bits;

        private ushort[] _data;
        private ChunkData _chunkData;

        public Chunk(ChunkPos chunkPos)
        {
            Pos = chunkPos;
        }

        public ulong ContentHash { get; private set; }

        public ChunkPos Pos { get; }

        public bool HasData
        {
            get
            {
                return _data != null;
            }
        }

        public ushort this[int x, int y, int z]
        {
            get
            {
                int index = GetIndex(x, y, z);
                return _data == null ? (ushort)0 : _data[index];
            }

            set
            {
                int index = GetIndex(x, y, z);
                if (_data == null)
                {
                    if (value == 0)
                    {
                        return;
                    }

                    _data = new ushort[Size * Size * Size];
                }

                ushort oldValue = _data[index];
                if (oldValue != value)
                {
                    ContentHash = ContentHash - GetCubeHash(index, oldValue) + GetCubeHash(index, value);
                    _data[index] = value;
                    _chunkData = null;
                }
            }
        }

        public ChunkData GetChunkData()
        {
            if (_chunkData == null)
            {
                _chunkData = new ChunkData() { Pos = Pos };

                if (_data != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        for (int i = 0; i < _data.Length; ++i)
                        {
                            ushort value = _data[i];
                            int length = 1;

                            while (length < 256 && i + length < _data.Length)
                            {
                                if (_data[i + length] == value)
                                {
                                    ++length;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            stream.WriteByte((byte)(length - 1));
                            stream.WriteByte((byte)value);
                            stream.WriteByte((byte)(value >> 8));

                            i += length - 1;
                        }

                        _chunkData.Data = stream.ToArray();
                    }
                }
            }

            return _chunkData;
        }

        public void PasteChunkData(ChunkData chunkData)
        {
            if (chunkData == null)
            {
                throw new ArgumentNullException("chunkData");
            }

            ContentHash = 0;

            if (chunkData.Data == null)
            {
                _data = null;
                return;
            }

            if (_data == null)
            {
                _data = new ushort[Size * Size * Size];
            }

            using (var stream = new MemoryStream(chunkData.Data))
            {
                int i = 0;
                while (i < _data.Length)
                {
                    int length = stream.ReadByte() + 1;
                    int v0 = stream.ReadByte();
                    int v1 = stream.ReadByte();

                    if (length <= 0 || v0 < 0 || v1 < 0)
                    {
                        throw new Exception("Invalid ChunkData");
                    }

                    ushort value = (ushort)((v1 << 8) | v0);
                    for (int j = i; j < i + length; ++j)
                    {
                        _data[j] = value;
                        ContentHash += GetCubeHash(j, value);
                    }

                    i += length;
                }

                if (i != _data.Length || stream.Position != stream.Length)
                {
                    throw new Exception("Invalid ChunkData");
                }
            }
        }

        private int GetIndex(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= Size || y >= Size || z >= Size)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (((x << Bits) + y) << Bits) + z;
        }

        private ulong GetCubeHash(int index, ushort value)
        {
            return (((ulong)index << 1) | 1) * value;
        }
    }
}
