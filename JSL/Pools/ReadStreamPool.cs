// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using JSL.Buffers;

namespace JSL.Pools
{
    /// <summary>
    /// Thread-unsafe pool for reusing <see cref="Buffers.ReadStream"/> instances to minimize garbage collection.
    /// </summary>
    public class ReadStreamPool
    {
        /// <summary>
        /// Retrieves an available read stream from the pool or instantiates a new one if empty.
        /// </summary>
        /// <returns>A clean, reset <see cref="Buffers.ReadStream"/> instance.</returns>
        public ReadStream Get()
        {
            if (_pools.Count == 0)
            {
                return new ReadStream();
            }
            var pool = _pools.Dequeue();
            pool.Reset();
            return pool;
        }

        /// <summary>
        /// Returns a read stream to the pool cache for future reuse.
        /// </summary>
        /// <param name="pool">The read stream instance to return.</param>
        public void Give(ReadStream pool)
        {
            _pools.Enqueue(pool);
        }

        private Queue<ReadStream> _pools = new Queue<ReadStream>();
    }
}