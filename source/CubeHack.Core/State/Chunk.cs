// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Geometry;
using CubeHack.Storage;
using System;
using System.IO;

namespace CubeHack.State
{
    /// <summary>
    /// Represents a fixed-size large cubic part of the game world.
    /// </summary>
    public sealed class Chunk
    {
        private readonly StorageKey _storageKey;
        private readonly ISaveFile _saveFile;
        private ushort[] _data;
        private ChunkData _chunkData;
        private bool _isCreated;

        public Chunk(World world, ChunkPos chunkPos)
        {
            World = world;
            Pos = chunkPos;

            _storageKey = StorageKey.Get("ChunkData", chunkPos);

            var saveFile = world?.Universe?.SaveFile;
            var savedValue = saveFile?.ReadAsync(_storageKey)?.Result;
            if (savedValue != null)
            {
                var chunkData = savedValue.Deserialize<ChunkData>();
                if (chunkData != null)
                {
                    try
                    {
                        PasteChunkData(chunkData);
                    }
                    catch
                    {
                        _data = null;
                        _isCreated = false;
                        _chunkData = null;
                        ContentHash = 0;
                    }
                }
            }

            _saveFile = saveFile;
        }

        public ulong ContentHash { get; private set; }

        public World World { get; }

        public ChunkPos Pos { get; }

        public bool IsCreated
        {
            get
            {
                return _isCreated;
            }

            set
            {
                if (_isCreated != value)
                {
                    _isCreated = value;
                    _chunkData = null;
                    Save();
                }
            }
        }

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

                    _data = new ushort[GeometryConstants.ChunkSize * GeometryConstants.ChunkSize * GeometryConstants.ChunkSize];
                }

                ushort oldValue = _data[index];
                if (oldValue != value)
                {
                    ContentHash = ContentHash - GetBlockHash(index, oldValue) + GetBlockHash(index, value);
                    _data[index] = value;
                    _chunkData = null;
                    Save();
                }
            }
        }

        public ChunkData GetChunkData()
        {
            if (_chunkData == null)
            {
                _chunkData = new ChunkData() { Pos = Pos, IsCreated = _isCreated };

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

            IsCreated = chunkData.IsCreated;
            PasteChunkData(chunkData.Data);
            _chunkData = chunkData;
            Save();
        }

        public void PasteChunkData(byte[] data)
        {
            ContentHash = 0;

            if (data == null)
            {
                _data = null;
                return;
            }

            if (_data == null)
            {
                _data = new ushort[GeometryConstants.ChunkSize * GeometryConstants.ChunkSize * GeometryConstants.ChunkSize];
            }

            using (var stream = new MemoryStream(data))
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
                        ContentHash += GetBlockHash(j, value);
                    }

                    i += length;
                }

                if (i != _data.Length || stream.Position != stream.Length)
                {
                    throw new Exception("Invalid ChunkData");
                }
            }
        }

        private void Save()
        {
            _saveFile?.Write(_storageKey, StorageValue.Serialize(GetChunkData()));
        }

        private int GetIndex(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= GeometryConstants.ChunkSize || y >= GeometryConstants.ChunkSize || z >= GeometryConstants.ChunkSize)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (((x << GeometryConstants.ChunkSizeBits) + y) << GeometryConstants.ChunkSizeBits) + z;
        }

        private ulong GetBlockHash(int index, ushort value)
        {
            return (((ulong)index << 1) | 1) * value;
        }
    }
}
