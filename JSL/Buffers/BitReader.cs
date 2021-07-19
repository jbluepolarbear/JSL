using JSL.Utility;

namespace JSL.Buffers
{
    public class BitReader
    {
        private readonly uint[] _data;
        private readonly int _numWords;
        private readonly int _numBits;
        private int _bitsRead;
        private ulong _scratch;
        private int _bitIndex;
        private int _wordIndex;
        private int _bitLength;
        private bool _overflow;

        public BitReader(uint[] data, int bytes)
        {
            Assert.NotNull(data);
            Assert.True(bytes % 4 == 0);
            _data = data;
            _numWords = bytes / 4;
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

        public void ReadBytes(byte[] data, int bytes)
        {
            Assert.True(GetAlignBits() == 0);

            if (_bitsRead + bytes * 8 >= _bitLength)
            {
                _overflow = true;
                return;
            }

            Assert.True(_bitIndex == 0 || _bitIndex == 8 || _bitIndex == 16 || _bitIndex == 24);

            int headBytes = (4 - _bitIndex / 8) % 4;
            if (headBytes > bytes)
            {
                headBytes = bytes;
            }

            for (var i = 0; i < headBytes; ++i)
            {
                data[i] = (byte) ReadBits(8);
            }

            if (headBytes == bytes)
            {
                return;
            }

            Assert.True(GetAlignBits() == 0);

            var numWords = (bytes - headBytes) / 4;
            if (numWords > 0)
            {
                Assert.True(_bitIndex == 0);
                // memcpy(data + headBytes, &_data[_wordIndex], numWords * 4);
                Memory.Copy(_data, data, _wordIndex, headBytes, numWords / 4);
                _bitsRead += numWords * 32;
                _wordIndex += numWords;
                _scratch = Memory.NetworkToHost(_data[_wordIndex]);
            }

            Assert.True(GetAlignBits() == 0);

            var tailStart = headBytes + numWords * 4;
            var tailBytes = bytes - tailStart;
            Assert.True(tailBytes >= 0 && tailBytes < 4);
            for (int i = 0; i < tailBytes; ++i)
            {
                data[tailStart + i] = (byte) ReadBits(8);
            }

            Assert.True(GetAlignBits() == 0);

            Assert.True(headBytes + numWords * 4 + tailBytes == bytes);
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

        public uint[] GetData()
        {
            return _data;
        }

        public bool IsOverflow()
        {
            return _overflow;
        }
    }
}