// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JSL.Pools
{
    /// <summary>
    /// Thread-safe class that interfaces with generic static list pools to reuse list allocations and avoid GC pressure.
    /// </summary>
    public class ListPool
    {
        private static readonly List<Action> _clearActions = new List<Action>();
        private static readonly object _globalLock = new object();

        /// <summary>
        /// Registers a cleanup callback for type-specific generic list pools.
        /// </summary>
        /// <param name="action">The cleanup delegate.</param>
        internal static void RegisterClearAction(Action action)
        {
            lock (_globalLock)
            {
                _clearActions.Add(action);
            }
        }

        /// <summary>
        /// Clears all generic list pool instances registered across the app.
        /// </summary>
        public static void Clear()
        {
            lock (_globalLock)
            {
                foreach (var action in _clearActions)
                {
                    action();
                }
            }
        }

        /// <summary>
        /// Retrieves a cleaned list instance of type <typeparamref name="T"/> from the generic cache.
        /// </summary>
        /// <typeparam name="T">The item type in the list.</typeparam>
        /// <returns>A recycled or newly allocated list.</returns>
        public List<T> Get<T>()
        {
            return ListPoolInternal<T>.Get();
        }

        /// <summary>
        /// Returns a list instance of type <typeparamref name="T"/> to the generic cache.
        /// Clears list elements before returning.
        /// </summary>
        /// <typeparam name="T">The item type in the list.</typeparam>
        /// <param name="list">The list instance to recycle.</param>
        public void Give<T>(List<T> list)
        {
            ListPoolInternal<T>.Give(list);
        }
    }

    /// <summary>
    /// Internal static generic list pool backing cache.
    /// </summary>
    /// <typeparam name="T">The item type in the list.</typeparam>
    internal static class ListPoolInternal<T>
    {
        private static readonly Queue<List<T>> _pool = new Queue<List<T>>(1024);
        private static readonly object _lock = new object();

        static ListPoolInternal()
        {
            ListPool.RegisterClearAction(Clear);
        }

        /// <summary>
        /// Thread-safely retrieves an available generic list from the cache queue.
        /// </summary>
        /// <returns>A clean list instance.</returns>
        public static List<T> Get()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    return _pool.Dequeue();
                }
            }
            return new List<T>();
        }

        /// <summary>
        /// Clears and enqueues a generic list back to the cache queue.
        /// </summary>
        /// <param name="list">The list instance to store.</param>
        public static void Give(List<T> list)
        {
            list.Clear();
            lock (_lock)
            {
                _pool.Enqueue(list);
            }
        }

        /// <summary>
        /// Clears all generic list instances stored in this type-specific pool.
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }
    }
}