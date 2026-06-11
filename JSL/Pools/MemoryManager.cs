// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.Utility;

namespace JSL.Pools
{
    /// <summary>
    /// Central manager orchestrating all pooling sub-systems in JSL (lists, messages, recyclables, etc.).
    /// </summary>
    public class MemoryManager: Singleton<MemoryManager>
    {
        /// <summary>
        /// Clears all stored instances inside JSL pools to release memory.
        /// </summary>
        public void Clear()
        {
            ListPool.Clear();
            RecyclablePool.Clear();
            GeneratedMessagePool.Clear();
        }
        
        /// <summary>
        /// Gets the pool used for recycling generic list instances.
        /// </summary>
        public ListPool ListPool { get; } = new ListPool();

        /// <summary>
        /// Gets the pool used for recycling standard network message wrappers.
        /// </summary>
        public NetMessagePool MessagePool { get; } = new NetMessagePool();

        /// <summary>
        /// Gets the pool used for recycling generic reference-counted network objects.
        /// </summary>
        public NetRecyclablePool RecyclablePool { get; } = new NetRecyclablePool();

        /// <summary>
        /// Gets the pool used for recycling dynamically generated messages.
        /// </summary>
        public NetGeneratedMessagePool GeneratedMessagePool { get; } = new NetGeneratedMessagePool();
    }
}