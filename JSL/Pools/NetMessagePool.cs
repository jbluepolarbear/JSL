// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.NetTypes;

namespace JSL.Pools
{
    /// <summary>
    /// Keeps a cache of reusable <see cref="NetMessage"/> instances to minimize garbage collection.
    /// </summary>
    public class NetMessagePool
    {
        /// <summary>
        /// Retrieves an available <see cref="NetMessage"/> from the pool.
        /// If a non-zero <paramref name="typeId"/> is specified, automatically loads the corresponding generated message.
        /// </summary>
        /// <param name="typeId">The class/type ID of the nested message, or 0 for empty.</param>
        /// <returns>A pooled <see cref="NetMessage"/> instance.</returns>
        public NetMessage Get(uint typeId = 0)
        {
            var netMessage = MemoryManager.Instance.RecyclablePool.Get<NetMessage>();
            if (typeId != 0)
            {
                netMessage.Message = MemoryManager.Instance.GeneratedMessagePool.Get(typeId);
            }
            return netMessage;
        }
    }
}