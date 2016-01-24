// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using CubeHack.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CubeHack.State
{
    public abstract class Container<TContainer>
        where TContainer : Container<TContainer>
    {
        private readonly ConcurrentDictionary<Type, byte[]> _components = new ConcurrentDictionary<Type, byte[]>();

        protected Container(Messenger messenger)
        {
            /* Simple check to make sure the CRTP is used correctly. */
            if (GetType() != typeof(TContainer)) throw new ArgumentException("Generic argument must be equal to the concrete class", nameof(TContainer));

            Messenger = messenger;
        }

        internal Messenger Messenger { get; }

        public TComponent Get<TComponent>()
        {
            byte[] bytes;
            _components.TryGetValue(typeof(TComponent), out bytes);
            return Serialization.Deserialize<TComponent>(bytes);
        }

        public bool TryGet<TComponent>(out TComponent component)
        {
            byte[] bytes;
            if (!_components.TryGetValue(typeof(TComponent), out bytes))
            {
                component = default(TComponent);
                return false;
            }

            component = Serialization.Deserialize<TComponent>(bytes);
            return true;
        }

        public void Set<TComponent>(TComponent component)
        {
            _components[typeof(TComponent)] = Serialization.Serialize(component);
            Messenger.Broadcast(new ComponentChangedMessage<TContainer, TComponent>((TContainer)this, component));
        }

        public void Remove<TComponent>()
        {
            ((IDictionary<Type, byte[]>)_components).Remove(typeof(TComponent));
        }

        public bool Has<TComponent>()
        {
            return _components.ContainsKey(typeof(TComponent));
        }
    }
}
