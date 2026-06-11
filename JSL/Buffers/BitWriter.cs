// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.Pooled;
using JSL.Utility;

namespace JSL.Buffers
{
    /// <summary>
    /// A high-performance unaligned bit-level writer over serialized uint word buffers.
    /// </summary>
    public class BitWriter
    {
        private readonly Array<uint> _data;
        private readonly int _numWords;
        private readonly int _numBits;
        private int _bitsWritten;
        private ulong _scratch;
        private int _bitIndex;
        private int _wordIndex;
        private bool _overflow;

        /// <summary>
        /// Instantiates a new bit writer with the specified buffer capacity in bytes.
        /// </summary>
        /// <param name="bytes">Capacity in bytes (must be a multiple of 4).</param>
        public BitWriter(int bytes)
        {
            Assert.True(bytes % 4 == 0);
            _data = new Array<uint>(bytes / 4);
            _numWords = bytes / 4;
            _numBits = _numWords * 32;
            _bitsWritten = 0;
            _scratch = 0;
            _bitIndex = 0;
            _wordIndex = 0;
            _overflow = false;
        }

        /// <summary>
        /// Resets the writer's bit pointer to the beginning of the buffer.
        /// </summary>
        public void Reset()
        {
            _bitsWritten = 0;
            _scratch = 0;
            _bitIndex = 0;
            _wordIndex = 0;
            _overflow = false;
        }

        /// <summary>
        /// Writes the specified number of bits from the value into the buffer.
        /// </summary>
        /// <param name="value">The unsigned integer containing the bits to write.</param>
        /// <param name="bits">Number of bits to write (1 to 32).</param>
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
        
        /// <summary>
        /// Pad with zero bits to align with the next byte boundary.
        /// </summary>
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
        
        /// <summary>
        /// Writes a block of bytes from the provided span into the buffer.
        /// Expects the writer to be byte-aligned.
        /// </summary>
        /// <param name="data">The source data buffer.</param>
        /// <param name="bytes">Number of bytes to write.</param>
        public void WriteBytes(ReadOnlySpan<byte> data, int bytes)
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
                Memory.Copy(data, _data.AsSpan(), headBytes, _wordIndex, numWords * 4);
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

        /// <summary>
        /// Flushes any partially written bits in the scratch pad to the underlying word buffer.
        /// </summary>
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
        
        /// <summary>
        /// Gets the number of bits needed to align to the next byte boundary.
        /// </summary>
        public int GetAlignBits()
        {
            return (8 - _bitsWritten % 8) % 8;
        }

        /// <summary>
        /// Gets the total number of bits written so far.
        /// </summary>
        public int GetBitsWritten()
        {
            return _bitsWritten;
        }

        /// <summary>
        /// Manually sets the writer's bit position.
        /// </summary>
        /// <param name="bitPosition">The new bit pointer index.</param>
        public void SetBitPosition(int bitPosition)
        {
            _bitsWritten = bitPosition;
            _wordIndex = _bitsWritten / 32;
            _bitIndex = _bitsWritten % 32;
            _scratch = (ulong) Memory.NetworkToHost(_data[_wordIndex]) << 32;
        }

        /// <summary>
        /// Gets the number of remaining writable bits.
        /// </summary>
        public int GetBitsAvailable()
        {
            return _numBits - _bitsWritten;
        }

        /// <summary>
        /// Gets the underlying uint word array buffer.
        /// </summary>
        public Array<uint> GetData()
        {
            return _data;
        }
        
        /// <summary>
        /// Returns the underlying buffer as a span of uint words.
        /// </summary>
        public Span<uint> AsSpan()
        {
            return _data.AsSpan();
        }
        
        /// <summary>
        /// Returns the underlying buffer as a read-only span of uint words.
        /// </summary>
        public ReadOnlySpan<uint> AsReadOnlySpan()
        {
            return _data.AsReadOnlySpan();
        }

        /// <summary>
        /// Gets the total bytes written (rounded up to nearest byte).
        /// </summary>
        public int GetBytesWritten()
        {
            return _bitsWritten / 8 + (_bitsWritten % 8 > 0 ? 1 : 0);
        }

        /// <summary>
        /// Gets the total byte capacity of the writer.
        /// </summary>
        public int GetTotalBytes()
        {
            return _numWords * 4;
        }

        /// <summary>
        /// Gets a value indicating whether a write operation overflowed the buffer capacity.
        /// </summary>
        public bool IsOverflow()
        {
            return _overflow;
        }
    }
}