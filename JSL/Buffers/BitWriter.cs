using JSL.Utility;

namespace JSL.Buffers
{
    public class BitWriter
    {
        private readonly uint[] _data;
        private readonly int _numWords;
        private readonly int _numBits;
        private int _bitsWritten;
        private ulong _scratch;
        private int _bitIndex;
        private int _wordIndex;
        private bool _overflow;

        public BitWriter(uint[] data, int bytes)
        {
            Assert.NotNull(data);
            Assert.True(bytes % 4 == 0);
            _data = data;
            _numWords = bytes / 4;
            _numBits = _numWords * 32;
            _bitsWritten = 0;
            _scratch = 0;
            _bitIndex = 0;
            _wordIndex = 0;
            _overflow = false;
        }

        public void Reset()
        {
            _bitsWritten = 0;
            _scratch = 0;
            _bitIndex = 0;
            _wordIndex = 0;
            _overflow = false;
        }

        public void WriteBits(uint value, int bits)
        {
            Assert.True(bits > 0);
            Assert.True(bits <= 32);
            Assert.True(_bitsWritten + bits <= _numBits);
            
            if (_bitsWritten + bits > _numBits)
            {
                _overflow = true;
                return;
            }

            value &= (uint) (((ulong) 1 << bits) - 1);

            _scratch |= (ulong) value << (64 - _bitIndex - bits);

            _bitIndex += bits;

            if (_bitIndex >= 32)
            {
                Assert.True(_wordIndex < _numWords);
                _data[_wordIndex] = Memory.HostToNetwork((uint) (_scratch >> 32));
                _scratch <<= 32;
                _bitIndex -= 32;
                _wordIndex++;
            }

            _bitsWritten += bits;
        }
        
        public void WriteAlign()
        {
            var remainderBits = _bitsWritten % 8;
            if (remainderBits != 0)
            {
                uint zero = 0;
                WriteBits(zero, 8 - remainderBits);
                Assert.True(_bitsWritten % 8 == 0);
            }
        }
        
        public void WriteBytes(byte[] data, int bytes)
        {
            Assert.True(GetAlignBits() == 0);
            if (_bitsWritten + bytes * 8 >=_numBits)
            {
               _overflow = true;
                return;
            }

            Assert.True(_bitIndex == 0 ||_bitIndex == 8 ||_bitIndex == 16 ||_bitIndex == 24);

            int headBytes = (4 -_bitIndex / 8) % 4;
            if (headBytes > bytes)
            {
                headBytes = bytes;
            }

            for (var i = 0; i < headBytes; ++i)
            {
                WriteBits(data[i], 8);
            }

            if (headBytes == bytes)
            {
                return;
            }

            Assert.True(GetAlignBits() == 0);

            int numWords = (bytes - headBytes) / 4;
            if (numWords > 0)
            {
                Assert.True(_bitIndex == 0);
                // memcpy(&_data[_wordIndex], data + headBytes, numWords * 4);
                Memory.Copy(data, _data, headBytes, _wordIndex, numWords * 4);
                _bitsWritten += numWords * 32;
                _wordIndex += numWords;
                _scratch = 0;
            }

            Assert.True(GetAlignBits() == 0);

            var tailStart = headBytes + numWords * 4;
            var tailBytes = bytes - tailStart;
            Assert.True(tailBytes >= 0 && tailBytes < 4);
            for (var i = 0; i < tailBytes; ++i)
            {
                WriteBits(data[tailStart + i], 8);
            }

            Assert.True(GetAlignBits() == 0);

            Assert.True(headBytes + numWords * 4 + tailBytes == bytes);
        }

        public void FlushBits()
        {
            if (_bitIndex != 0)
            {
                Assert.True(_wordIndex < _numWords);
                if (_wordIndex >= _numWords)
                {
                    _overflow = true;
                    return;
                }
                _data[_wordIndex] = Memory.HostToNetwork((uint) (_scratch >> 32));
            }
        }
        
        public int GetAlignBits()
        {
            return (8 - _bitsWritten % 8) % 8;
        }

        public int GetBitsWritten()
        {
            return _bitsWritten;
        }

        public void SetBitPosition(int bitPosition)
        {
            _bitsWritten = bitPosition;
            _wordIndex = _bitsWritten / 32;
            _bitIndex = _bitsWritten % 32;
            _scratch = (ulong) Memory.NetworkToHost(_data[_wordIndex]) << 32;
        }

        public int GetBitsAvailable()
        {
            return _numBits - _bitsWritten;
        }

        public uint[] GetData()
        {
            return _data;
        }

        public int GetBytesWritten()
        {
            return _bitsWritten / 8 + (_bitsWritten % 8 > 0 ? 1 : 0);
        }

        public int GetTotalBytes()
        {
            return _numWords * 4;
        }

        public bool IsOverflow()
        {
            return _overflow;
        }
    }
}