// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using JSL.NetTypes;

namespace JSL.Pools
{
    /// <summary>
    /// Thread-safe generic pool designed for recycling specific <see cref="NetRecyclable"/> resources.
    /// Uses an event callback mechanism to enqueue items back into the pool automatically when their reference count drops to 0.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="NetRecyclable"/> class to pool.</typeparam>
    public class NetBasePool<T> where T: NetRecyclable
    {
        private const int ReserveSize = 1024;
        private readonly Queue<T> _availableQueue = new Queue<T>(ReserveSize);
        private readonly Func<T> _allocator;
        private readonly object _lock = new object();

        /// <summary>
        /// Instantiates a new pool instance using the provided allocator function.
        /// </summary>
        /// <param name="allocator">The factory method to allocate new instances of type <typeparamref name="T"/>.</param>
        public NetBasePool(Func<T> allocator)
        {
            _allocator = allocator;
        }

        /// <summary>
        /// Retrieves an available instance of <typeparamref name="T"/> from the pool, or allocates a new one if the cache is empty.
        /// Automatically wires up the <see cref="NetRecyclable.OnRelease"/> event to recycle the instance on disposal.
        /// </summary>
        /// <returns>An active reference-counted instance of <typeparamref name="T"/>.</returns>
        public T Get()
        {
            T outInstance = null;
            lock (_lock)
            {
                if (_availableQueue.Count > 0)
                {
                    outInstance = _availableQueue.Dequeue();
                }
            }
            
            if (outInstance == null)
            {
                outInstance = _allocator();
                outInstance.OnRelease = (item) =>
                {
                    lock (_lock)
                    {
                        _availableQueue.Enqueue((T)item);
                    }
                };
            }
            
            outInstance.Acquire();
            return outInstance;
        }
    }
}