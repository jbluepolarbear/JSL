using System.Collections.Generic;
using JSL.Buffers;

namespace JSL.Pools
{
    public class WriteStreamPool
    {
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

        public void Give(WriteStream pool)
        {
            _pools.Enqueue(pool);
        }

        private Queue<WriteStream> _pools = new Queue<WriteStream>();
    }
}