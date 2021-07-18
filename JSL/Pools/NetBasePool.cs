using System;
using System.Collections.Generic;
using JSL.NetTypes;

namespace JSL.Pools
{
    public class NetBasePool<T> where T: NetRecyclable
    {
        private const int ReserveSize = 1024;
        private List<T> _availableList = new List<T>(ReserveSize);
        private Func<T> _allocator;

        public NetBasePool(Func<T> allocator)
        {
            _allocator = allocator;
        }

        public T Get()
        {
            T outInstance = null;
            if (_availableList.Count > 0)
            {
                foreach (var instance in _availableList)
                {
                    if (instance.RefCount == 0)
                    {
                        outInstance = instance;
                    }
                }
            }
            
            if (outInstance == null)
            {
                outInstance = _allocator();
                _availableList.Add(outInstance);
            }
            
            outInstance.Acquire();
            return outInstance;
        }
    }
}