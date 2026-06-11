// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace JSL.Pooled
{
    public class Array<T> : ICloneable, IList<T>, IStructuralComparable, IStructuralEquatable
    {
        private T[] _array;
        private int _length;
        
        public Array(int length = 0)
        {
            Resize(length);
        }
        
        public Array(Array<T> other)
        {
            Resize(other.Length);
            other.AsSpan().CopyTo(AsSpan());
        }

        public Array(ReadOnlySpan<T> span)
        {
            Resize(span.Length);
            span.CopyTo(AsSpan());
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        T IList<T>.this[int index]
        {
            get => this[index];
            set => this[index] = value;
        }
        
        public ref T this[int index] => ref _array[index];
        
        public int Length => _length;
        
        public Span<T> AsSpan() => AsSpan(0, _length);
        public Span<T> AsSpan(int start, int length) => new Span<T>(_array, start, length);
        public ReadOnlySpan<T> AsReadOnlySpan() => AsReadOnlySpan(0, _length);
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => new ReadOnlySpan<T>(_array, start, length);
        
        public int IndexOf(in T value)
        {
            for (var i = 0; i < _length; ++i)
            {
                if (_array[i].Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }

        int IList<T>.IndexOf(T item)
        {
            return IndexOf(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_array).GetEnumerator();
        }

        ~Array()
        {
            ReturnArray();
        }

        public int CompareTo(object other, IComparer comparer)
        {
            return comparer.Compare(other, comparer);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public object Clone()
        {
            return new Array<T>(this);
        }

        public void Resize(int length)
        {
            if (_array == null || length > _array.Length)
            {
                RentArray(length);
            }
            else if (_array.Length >= length && _length <= length)
            {
                _length = length;
            }
            else
            {
                _length = length;
            }
        }
        
        private void RentArray(int length)
        {
            if (_array != null)
            {
                ReturnArray();
            }
            
            var newArray = ArrayPool<T>.Shared.Rent(length);
            
            _array = newArray;
            _length = length;
        }

        private void ReturnArray()
        {
            if (_array == null)
            {
                return;
            }
            
            ArrayPool<T>.Shared.Return(_array);
            _array = null;
            _length = 0;
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            ReturnArray();
            Resize(0);
        }
        
        public bool Contains(in T value)
        {
            return IndexOf(value) != -1;
        }

        bool ICollection<T>.Contains(T item)
        {
            return Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _array.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public int Count => _length;
        public bool IsReadOnly => false;
        public bool Equals(object other, IEqualityComparer comparer)
        {
            return comparer.Equals(other, comparer);
        }

        public int GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable) _array).GetHashCode(comparer);
        }
    }
}