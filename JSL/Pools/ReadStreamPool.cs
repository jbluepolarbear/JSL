using System.Collections.Generic;
using JSL.Buffers;

namespace JSL.Pools
{
    public class ReadStreamPool
    {
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

        public void Give(ReadStream pool)
        {
            _pools.Enqueue(pool);
        }

        private Queue<ReadStream> _pools = new Queue<ReadStream>();
    }
}