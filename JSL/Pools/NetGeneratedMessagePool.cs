// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using JSL.Messages;

namespace JSL.Pools
{
    /// <summary>
    /// Thread-safe pool managing dynamically generated subclasses of <see cref="BaseMessage"/> by type ID.
    /// </summary>
    public class NetGeneratedMessagePool
    {
        /// <summary>
        /// Delegate factory method to instantiate concrete <see cref="BaseMessage"/> types by class ID.
        /// </summary>
        /// <param name="classId">The type ID of the message class.</param>
        /// <returns>A new <see cref="BaseMessage"/> instance.</returns>
        public delegate BaseMessage Factory(uint classId);

        private Factory _messageFactory;
        private readonly object _lock = new object();
        private Dictionary<uint, NetBasePool<BaseMessage>> _messagePools = new Dictionary<uint, NetBasePool<BaseMessage>>();

        /// <summary>
        /// Registers the generation factory callback to resolve message instantiation dynamically.
        /// </summary>
        /// <param name="factory">The message factory delegate.</param>
        public void RegisterMessageFactory(Factory factory)
        {
            lock (_lock)
            {
                _messageFactory = factory;
            }
        }

        /// <summary>
        /// Clears all message sub-pools and unregistered generation factory callbacks.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _messageFactory = null;
                _messagePools.Clear();
            }
        }

        /// <summary>
        /// Retrieves a pooled dynamically generated message of the specified class ID.
        /// </summary>
        /// <param name="typeId">The class/type ID of the message to retrieve.</param>
        /// <returns>A recycled or newly instantiated message.</returns>
        public BaseMessage Get(uint typeId)
        {
            NetBasePool<BaseMessage> messagePool;
            lock (_lock)
            {
                if (!_messagePools.TryGetValue(typeId, out messagePool))
                {
                    messagePool = new NetBasePool<BaseMessage>(() => MessageFromTypeId(typeId));
                    _messagePools.Add(typeId, messagePool);
                }
            }
            var message = messagePool.Get();
            return message;
        }

        private BaseMessage MessageFromTypeId(uint typeId)
        {
            Factory factory;
            lock (_lock)
            {
                factory = _messageFactory;
            }
            return factory(typeId);
        }
    }
}