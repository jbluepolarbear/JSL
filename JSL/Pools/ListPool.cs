using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JSL.Pools
{
    public class ListPool
    {
        private int _reserveSize = 1024;
        private Dictionary<Type, List<IList>> _availableLists = new Dictionary<Type, List<IList>>();

        public List<T> Get<T>()
        {
            if (!_availableLists.TryGetValue(typeof(T), out var listPool))
            {
                listPool = new List<List<T>>(_reserveSize).Cast<IList>().ToList();
                _availableLists.Add(typeof(T), listPool);
            }

            if (listPool.Count > 0)
            {
                var list = listPool[listPool.Count - 1];
                listPool.RemoveAt(listPool.Count - 1);
                return (List<T>) list;
            }
            return new List<T>();
        }

        public void Give<T>(List<T> list)
        {
            if (!_availableLists.TryGetValue(typeof(T), out var listPool))
            {
                listPool = new List<IList>(_reserveSize);
                _availableLists.Add(typeof(T), listPool);
            }
            list.Clear();
            listPool.Add(list);
        }
    }
}