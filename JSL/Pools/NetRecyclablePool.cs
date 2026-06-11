// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using JSL.NetTypes;

namespace JSL.Pools
{
    /// <summary>
    /// Thread-safe pool managing multiple sub-pools categorized by <see cref="Type"/> to recycle network recyclable types.
    /// </summary>
    public class NetRecyclablePool
    {
        private Dictionary<Type, NetBasePool<NetRecyclable>> _recyclePools = new Dictionary<Type, NetBasePool<NetRecyclable>>();
        private readonly object _lock = new object();

        /// <summary>
        /// Clears all internal recyclable pools.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _recyclePools.Clear();
            }
        }

        /// <summary>
        /// Retrieves or creates a pooled recyclable instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="NetRecyclable"/> to retrieve.</typeparam>
        /// <returns>A recycled or newly allocated instance of <typeparamref name="T"/>.</returns>
        public T Get<T>() where T: NetRecyclable, new()
        {
            var type = typeof(T);
            NetBasePool<NetRecyclable> recyclePool;
            lock (_lock)
            {
                if (!_recyclePools.TryGetValue(type, out recyclePool))
                {
                    recyclePool = new NetBasePool<NetRecyclable>(() => new T());
                    _recyclePools.Add(type, recyclePool);
                }
            }
            return (T) recyclePool.Get();
        }
    }
}