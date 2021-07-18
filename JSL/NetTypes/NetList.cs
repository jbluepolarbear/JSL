using System.Collections.Generic;
using JSL.Buffers;
using JSL.Pools;

namespace JSL.NetTypes
{
    /// <summary>
    /// A Generic Network Array
    /// Will not work for Generated Messages and will cause exceptions on Deserialize
    /// Use NetArray<NetMessage> to wrap a generated Message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetList<T>: NetRecyclable where T: NetRecyclable, new ()
    {
        private List<T> _list { get; set; }
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

        public int Count => _list?.Count ?? 0;

        public T this[int i]
        {
            get => _list[i];
            private set => _list[i] = value;
        }

        public void Add(T item)
        {
            item.Acquire();
            _list.Add(item);
        }

        protected override void AcquireImpl()
        {
            base.AcquireImpl();
            _list = MemoryManager.Instance.ListPool.Get<T>();
        }

        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Clear();
            MemoryManager.Instance.ListPool.Give(_list);
            _list = null;
        }
    }
}