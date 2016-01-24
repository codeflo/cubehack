// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Storage;
using System;
using System.Collections.Generic;

namespace CubeHack.State
{
    public sealed class Universe : Container<Universe>, IDisposable
    {
        private readonly HashSet<Entity> _entities = new HashSet<Entity>();

        private volatile bool _isDisposed;

        public Universe(ISaveFile saveFile)
            : base(new Messenger())
        {
            SaveFile = saveFile;

            StartWorld = new World(this);
        }

        public ISaveFile SaveFile { get; }

        public World StartWorld { get; }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                SaveFile.Dispose();
            }
        }

        public IEnumerable<Entity> GetEntities()
        {
            return _entities;
        }

        public IEnumerable<Entity> GetEntitiesWithComponent<TComponent>()
        {
            foreach (var entity in _entities)
            {
                if (entity.Has<TComponent>())
                {
                    yield return entity;
                }
            }
        }

        internal void AddEntity(Entity entity)
        {
            if (entity.Universe != this) throw new InvalidOperationException("Can't manually add an entity to the universe. Use the Entity.Universe property.");
            _entities.Add(entity);
        }

        internal void RemoveEntity(Entity entity)
        {
            if (entity.Universe == this) throw new InvalidOperationException("Can't manually remove an entity from the universe. Use the Entity.Universe property.");
            _entities.Remove(entity);
        }
    }
}
