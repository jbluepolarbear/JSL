using System;
using System.Collections.Generic;
using JSL.NetTypes;
using JSL.Pools;

namespace JSL.Buffers
{
    public class RingBuffer<T>: IDisposable where T: NetRecyclable
    {
        private List<T> _list;
        private readonly int _capacity;
        private int _tail;

        /// <summary>
        /// Ring Buffer is a constant size and new values will wrap around.
        /// </summary>
        /// <param name="capacity"></param>
        public RingBuffer(int capacity)
        {
            _capacity = capacity;
            _tail = 0;
        }

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

        public int Count => List.Count;

        public bool Full => List.Count == _capacity;

        /// <summary>
        /// 0 is the tail or most current value
        /// less than 0 will go in reverse add order
        /// greater than 0 starts at the oldest value
        /// </summary>
        /// <param name="i"></param>
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

        public void Dispose()
        {
            if (_list != null)
            {
                MemoryManager.Instance.ListPool.Give(_list);
                _list = null;
            }
        }
    }
}