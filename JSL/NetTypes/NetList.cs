// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using JSL.Buffers;
using JSL.Pools;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents a generic network list of elements of type <typeparamref name="T"/> that implements network serialization.
    /// Will not work for generated messages and will cause exceptions on Deserialize.
    /// Use NetArray&lt;NetMessage&gt; to wrap a generated message.
    /// </summary>
    /// <typeparam name="T">The type of items in the list, which must be a recyclable serializable network type with a parameterless constructor.</typeparam>
    public class NetList<T> : NetRecyclableSerializable where T : NetRecyclableSerializable, new ()
    {
        /// <summary>
        /// Gets or sets the underlying list storage.
        /// </summary>
        private List<T> _list { get; set; }

        /// <summary>
        /// Serializes the list length and all elements to the write stream.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            var count = 0;
            if (_list != null)
            {
                count = _list.Count;
            }
            writer.WriteBits((uint) count, 16);
            if (_list == null)
            {
                return;
            }
            foreach (var item in _list)
            {
                item.Serialize(writer);
            }
        }

        /// <summary>
        /// Deserializes the list length and elements from the read stream.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            Clear();
            var count = (int) reader.ReadBits(16);
            if (count == 0)
            {
                return;
            }

            for (var i = 0; i < count; ++i)
            {
                using var instance = MemoryManager.RecyclablePool.Get<T>();
                instance.Deserialize(reader);
                Add(instance);
            }
        }

        /// <summary>
        /// Clears all elements in the list and disposes/releases them back to the pool.
        /// </summary>
        public void Clear()
        {
            if (_list == null)
            {
                return;
            }
            foreach (var item in _list)
            {
                item.Dispose();
            }
            _list.Clear();
        }

        /// <summary>
        /// Gets the number of elements contained in the list.
        /// </summary>
        public int Count => _list?.Count ?? 0;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="i">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int i]
        {
            get => _list[i];
            private set => _list[i] = value;
        }

        /// <summary>
        /// Adds an item to the list and increments its reference count.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            item.Acquire();
            _list.Add(item);
        }

        /// <summary>
        /// Custom implementation for acquiring the list instance from the pool.
        /// Gets a pooled list instance from the memory manager.
        /// </summary>
        protected override void AcquireImpl()
        {
            base.AcquireImpl();
            _list = MemoryManager.Instance.ListPool.Get<T>();
        }

        /// <summary>
        /// Custom implementation for releasing the list instance back to the pool.
        /// Clears and recycles the list storage back to the memory manager.
        /// </summary>
        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Clear();
            MemoryManager.Instance.ListPool.Give(_list);
            _list = null;
        }
    }
}