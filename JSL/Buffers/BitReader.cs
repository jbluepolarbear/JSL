using System;
using JSL.Pooled;
using JSL.Utility;

namespace JSL.Buffers
{
    public class BitReader
    {
        private readonly Array<uint> _data;
        private readonly int _numWords;
        private readonly int _numBits;
        private int _bitsRead;
        private ulong _scratch;
        private int _bitIndex;
        private int _wordIndex;
        private int _bitLength;
        private bool _overflow;

        public BitReader(int bytes)
        {
            Assert.True(bytes % 4 == 0);
            _data = new Array<uint>(bytes / 4);
            _numWords = bytes / 4;
            _numBits = _numWords * 32;
            _bitsRead = 0;
            _bitIndex = 0;
            _wordIndex = 0;
            _bitLength = _numBits;
            _scratch = Memory.NetworkToHost(_data[0]);
            _overflow = false;
        }
        
        public BitReader(ReadOnlySpan<uint> data)
        {
            _data = new Array<uint>(data);
            _numWords = _data.Length;
            _numBits = _numWords * 32;
            _bitsRead = 0;
            _bitIndex = 0;
            _wordIndex = 0;
            _bitLength = _numBits;
            _scratch = Memory.NetworkToHost(_data[0]);
            _overflow = false;
        }

        public void Reset()
        {
            _bitsRead = 0;
            _bitIndex = 0;
            _wordIndex = 0;
            _bitLength = _numBits;
            _scratch = Memory.NetworkToHost(_data[0]);
            _overflow = false;
        }

        public uint ReadBits(int bits)
        {
            Assert.True(bits > 0);
            Assert.True(bits <= 32);
            Assert.True(_bitsRead + bits <= _bitLength);

            if (_bitsRead + bits > _bitLength)
            {
                _overflow = true;
                return 0;
            }

            _bitsRead += bits;

            Assert.True(_bitIndex < 32);

            if (_bitIndex + bits < 32)
            {
                _scratch <<= bits;
                _bitIndex += bits;
            }
            else
            {
                _wordIndex++;
                Assert.True(_wordIndex < _numWords);
                var a = 32 - _bitIndex;
                var b = bits - a;
                _scratch <<= a;
                _scratch |= Memory.NetworkToHost(_data[_wordIndex]);
                _scratch <<= b;
                _bitIndex = b;
            }

            var output = (uint) (_scratch >> 32);

            _scratch &= 0xFFFFFFFF;

            return output;
        }

        public void ReadAlign()
        {
            var remainderBits = _bitsRead % 8;
            if (remainderBits != 0)
            {
                #if NDEBUG
                ReadBits(8 - remainderBits);
                #else
                var value = ReadBits(8 - remainderBits);
                Assert.True(value == 0);
                Assert.True(_bitsRead % 8 == 0);
                #endif
            }
        }

        public void ReadBytes(Span<byte> data)
        {
            Assert.True(GetAlignBits() == 0);

            if (_bitsRead + data.Length * 8 >= _bitLength)
            {
                _overflow = true;
                return;
            }

            Assert.True(_bitIndex == 0 || _bitIndex == 8 || _bitIndex == 16 || _bitIndex == 24);

            int headBytes = (4 - _bitIndex / 8) % 4;
            if (headBytes > data.Length)
            {
                headBytes = data.Length;
            }

            for (var i = 0; i < headBytes; ++i)
            {
                data[i] = (byte) ReadBits(8);
            }

            if (headBytes == data.Length)
            {
                return;
            }

            Assert.True(GetAlignBits() == 0);

            var numWords = (data.Length - headBytes) / 4;
            if (numWords > 0)
            {
                Assert.True(_bitIndex == 0);
                // memcpy(data + headBytes, &_data[_wordIndex], numWords * 4);
                Memory.Copy(_data.AsSpan(), data, _wordIndex, headBytes, numWords * 4);
                _bitsRead += numWords * 32;
                _wordIndex += numWords;
                _scratch = Memory.NetworkToHost(_data[_wordIndex]);
            }

            Assert.True(GetAlignBits() == 0);

            var tailStart = headBytes + numWords * 4;
            var tailBytes = data.Length - tailStart;
            Assert.True(tailBytes >= 0 && tailBytes < 4);
            for (int i = 0; i < tailBytes; ++i)
            {
                data[tailStart + i] = (byte) ReadBits(8);
            }

            Assert.True(GetAlignBits() == 0);

            Assert.True(headBytes + numWords * 4 + tailBytes == data.Length);
        }

        public int GetAlignBits()
        {
            return (8 - _bitsRead % 8) % 8;
        }

        public int GetBitsRead()
        {
            return _bitsRead;
        }
        
        private int GetBytesProcessed()
        {
            return _bitsRead / 8 + (_bitsRead % 8 > 0 ? 1 : 0);
        }

        public void SetBitLength(int bitLength)
        {
            Assert.True(bitLength <= _numBits);
            _bitLength = bitLength;
        }

        public int GetBytesRead()
        {
            return (_wordIndex + 1) * 4;
        }

        public int GetBitsRemaining()
        {
            return _numBits - _bitsRead;
        }

        public int GetTotalBits() 
        {
            return _numBits;
        }

        public int GetTotalBytes()
        {
            return _numBits * 8;
        }

        public Array<uint> GetData()
        {
            return _data;
        }
        
        public Span<uint> AsSpan()
        {
            return _data.AsSpan();
        }
        
        public ReadOnlySpan<uint> AsReadOnlySpan()
        {
            return _data.AsReadOnlySpan();
        }

        public bool IsOverflow()
        {
            return _overflow;
        }
    }
}