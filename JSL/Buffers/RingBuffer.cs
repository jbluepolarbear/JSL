// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using JSL.NetTypes;
using JSL.Pools;

namespace JSL.Buffers
{
    /// <summary>
    /// A pooled, constant-sized ring buffer that automatically disposes evicted elements.
    /// </summary>
    /// <typeparam name="T">The pooled element type deriving from <see cref="NetRecyclable"/>.</typeparam>
    public class RingBuffer<T>: IDisposable where T: NetRecyclable
    {
        private List<T> _list;
        private readonly int _capacity;
        private int _tail;

        /// <summary>
        /// Instantiates a new ring buffer with the specified capacity.
        /// </summary>
        /// <param name="capacity">Maximum number of elements in the ring buffer.</param>
        public RingBuffer(int capacity)
        {
            _capacity = capacity;
            _tail = 0;
        }

        /// <summary>
        /// Adds a new value to the ring buffer. Evicts and disposes of the oldest item if capacity is reached.
        /// </summary>
        /// <param name="value">The element to add.</param>
        public void Add(T value)
        {
            if (List.Count < _capacity)
            {
                value.Acquire();
                List.Add(value);
                _tail = List.Count - 1;
                return;
            }

            _tail = (_tail + 1) % _capacity;
            List[_tail].Dispose();
            value.Acquire();
            List[_tail] = value;
        }

        /// <summary>
        /// Clears the ring buffer and disposes of all stored elements to prevent leaks.
        /// </summary>
        public void Clear()
        {
            if (_list != null)
            {
                foreach (var item in _list)
                {
                    item?.Dispose();
                }
                _list.Clear();
            }
            _tail = 0;
        }

        /// <summary>
        /// Gets the current number of elements stored.
        /// </summary>
        public int Count => List.Count;

        /// <summary>
        /// Gets a value indicating whether the ring buffer is at full capacity.
        /// </summary>
        public bool Full => List.Count == _capacity;

        /// <summary>
        /// Gets an element relative to the current tail:
        /// 0 represents the tail (most current element),
        /// less than 0 represents elements in reverse order,
        /// greater than 0 starts from the oldest element.
        /// </summary>
        /// <param name="i">The index offset.</param>
        /// <returns>The pooled element at the offset.</returns>
        public T this[int i]
        {
            get
            {
                var index = _tail - i;
                if (index < 0)
                {
                    index += List.Count;
                }
                else if (index >= List.Count)
                {
                    index -= List.Count;
                }
                return List[index];
            }
            private set => List[i] = value;
        }

        /// <summary>
        /// Gets the underlying pooled generic list container.
        /// </summary>
        private List<T> List
        {
            get
            {
                if (_list == null)
                {
                    _list = MemoryManager.Instance.ListPool.Get<T>();
                    _list.Clear();
                }

                return _list;
            }
        }

        /// <summary>
        /// Disposes of the ring buffer, disposing all inner items and returning the list to the list pool.
        /// </summary>
        public void Dispose()
        {
            if (_list != null)
            {
                foreach (var item in _list)
                {
                    item?.Dispose();
                }
                MemoryManager.Instance.ListPool.Give(_list);
                _list = null;
            }
        }
    }
}