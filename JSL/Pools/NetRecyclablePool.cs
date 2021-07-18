using System;
using System.Collections.Generic;
using JSL.NetTypes;

namespace JSL.Pools
{
    public class NetRecyclablePool
    {
        private Dictionary<Type, NetBasePool<NetRecyclable>> _recyclePools = new Dictionary<Type, NetBasePool<NetRecyclable>>();

        public T Get<T>() where T: NetRecyclable, new()
        {
            var type = typeof(T);
            if (!TryGet<T>(out var recyclePool))
            {
                recyclePool = new NetBasePool<NetRecyclable>(() => new T());
                _recyclePools.Add(type, recyclePool);
            }
            return (T) recyclePool.Get();
        }

        private bool TryGet<T>(out NetBasePool<NetRecyclable> outPool) where T: NetRecyclable, new()
        {
            foreach (var pool in _recyclePools)
            {
                if (pool.Key == typeof(T))
                {
                    outPool = pool.Value;
                    return true;
                }
            }

            outPool = null;
            return false;
        }
    }
}