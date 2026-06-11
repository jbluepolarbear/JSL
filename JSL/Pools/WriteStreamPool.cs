// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using JSL.Buffers;

namespace JSL.Pools
{
    /// <summary>
    /// Thread-unsafe pool for reusing <see cref="Buffers.WriteStream"/> instances to minimize garbage collection.
    /// </summary>
    public class WriteStreamPool
    {
        /// <summary>
        /// Retrieves an available write stream from the pool or instantiates a new one if empty.
        /// </summary>
        /// <returns>A clean, reset <see cref="Buffers.WriteStream"/> instance.</returns>
        public WriteStream Get()
        {
            if (_pools.Count == 0)
            {
                return new WriteStream();
            }
            var pool = _pools.Dequeue();
            pool.Reset();
            return pool;
        }

        /// <summary>
        /// Returns a write stream to the pool cache for future reuse.
        /// </summary>
        /// <param name="pool">The write stream instance to return.</param>
        public void Give(WriteStream pool)
        {
            _pools.Enqueue(pool);
        }

        private Queue<WriteStream> _pools = new Queue<WriteStream>();
    }
}