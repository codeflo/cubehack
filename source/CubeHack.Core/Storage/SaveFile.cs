// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CubeHack.Storage
{
    public sealed class SaveFile : ISaveFile
    {
        /*
         * This implements a very simple file-based key/value store. Every key/value entry is stored in its own file,
         * with the filename based on the SHA-1 hash of the key.
         *
         * Obviously, this scheme will become inefficient once save files get larger, so we will eventually need
         * to figure out something more efficient.
         */

        private const int _retries = 10;

        private static readonly TimeSpan _saveInterval = TimeSpan.FromSeconds(30);

        private readonly object _mutex = new object();
        private readonly BlockingCollection<StorageKey> _queuedKeys = new BlockingCollection<StorageKey>();
        private readonly Dictionary<StorageKey, StorageValue> _queuedValues = new Dictionary<StorageKey, StorageValue>();
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly Thread _writeThread;
        private readonly string _path;

        public SaveFile(string path)
        {
            _path = path;
            _writeThread = new Thread(RunWriteThread);
            _writeThread.Name = nameof(SaveFile) + " Write Thread";
            _writeThread.IsBackground = true;
            _writeThread.Start();
        }

        public void Dispose()
        {
            _cancellation.Cancel();
            _writeThread.Join();
            _cancellation.Dispose();
            _queuedKeys.Dispose();
        }

        public async Task<StorageValue> ReadAsync(StorageKey key)
        {
            for (int i = 0; i < _retries; ++i)
            {
                if (_cancellation.IsCancellationRequested) throw new ObjectDisposedException(nameof(SaveFile));

                lock (_mutex)
                {
                    /* If we find the value in the write queue, return the queued value. (It's more recent than what's on disk.) */
                    StorageValue value;
                    if (_queuedValues.TryGetValue(key, out value)) return value;
                }

                var filename = GetFilename(key);
                if (!File.Exists(filename)) return null;

                try
                {
                    using (var stream = File.Open(GetFilename(key), FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var memoryStream = new MemoryStream((int)stream.Length))
                        {
                            await stream.CopyToAsync(memoryStream);
                            return new StorageValue(memoryStream.ToArray());
                        }
                    }
                }
                catch (IOException)
                {
                }
            }

            /* Ignore read errors (for now). */
            return null;
        }

        public void Write(StorageKey key, StorageValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            lock (_mutex)
            {
                _queuedValues[key] = value;
            }

            _queuedKeys.Add(key);
        }

        private void RunWriteThread()
        {
            try
            {
                var cancellationToken = _cancellation.Token;
                while (true)
                {
                    if (_queuedKeys.Count == 0)
                    {
                        if (cancellationToken.WaitHandle.WaitOne(_saveInterval)) throw new OperationCanceledException();
                    }

                    var key = _queuedKeys.Take(cancellationToken);
                    SaveValue(key);
                }
            }
            catch (OperationCanceledException)
            {
                /* After we were interrupted, still write all queued values to disk. */

                StorageKey key;
                while (_queuedKeys.TryTake(out key))
                {
                    SaveValue(key);
                }
            }
        }

        private void SaveValue(StorageKey key)
        {
            StorageValue value;
            lock (_mutex)
            {
                if (!_queuedValues.TryGetValue(key, out value)) return;
            }

            string filename = GetFilename(key);
            string directoryName = Path.GetDirectoryName(filename);

            for (int i = 0; i < _retries; ++i)
            {
                try
                {
                    Directory.CreateDirectory(directoryName);

                    using (var stream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        stream.Write(value.Bytes, 0, value.Bytes.Length);
                    }

                    break;
                }
                catch (IOException)
                {
                }
            }

            lock (_mutex)
            {
                StorageValue newValue;
                if (_queuedValues.TryGetValue(key, out newValue) && object.ReferenceEquals(value, newValue))
                {
                    /* Value is still current; remove it from the queue. */
                    _queuedValues.Remove(key);
                }
            }
        }

        private string GetFilename(StorageKey key)
        {
            using (var sha1 = new SHA1Managed())
            {
                var hashBytes = sha1.ComputeHash(key.Bytes);
                return Path.Combine(
                    _path,
                    HexConverter.ToHex(hashBytes, 0, 1),
                    HexConverter.ToHex(hashBytes, 1, hashBytes.Length - 2)) + ".cubedata";
            }
        }
    }
}
