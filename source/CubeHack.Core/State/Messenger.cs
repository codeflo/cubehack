// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.Collections.Generic;

namespace CubeHack.State
{
    public sealed class Messenger
    {
        private readonly object _mutex = new object();
        private readonly Dictionary<Type, object> _messageHandlers = new Dictionary<Type, object>();

        public void Register<TMessage>(Action<TMessage> messageHandler)
        {
            lock (_mutex)
            {
                object oldMessageHandler;
                _messageHandlers.TryGetValue(typeof(TMessage), out oldMessageHandler);
                _messageHandlers[typeof(TMessage)] = (Action<TMessage>)oldMessageHandler + messageHandler;
            }
        }

        internal void Broadcast<TMessage>(TMessage message)
        {
            object messageHandler;
            lock (_mutex)
            {
                _messageHandlers.TryGetValue(typeof(TMessage), out messageHandler);
            }

            if (messageHandler != null)
            {
                ((Action<TMessage>)messageHandler)(message);
            }
        }
    }
}
