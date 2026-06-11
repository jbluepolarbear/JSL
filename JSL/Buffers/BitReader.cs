// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.Pooled;
using JSL.Utility;

namespace JSL.Buffers
{
    /// <summary>
    /// A high-performance unaligned bit-level reader over serialized uint word buffers.
    /// </summary>
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

        /// <summary>
        /// Instantiates a new bit reader with the specified buffer capacity in bytes.
        /// </summary>
        /// <param name="bytes">Capacity in bytes (must be a multiple of 4).</param>
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
        
        /// <summary>
        /// Instantiates a new bit reader wrapped around an existing read-only data buffer span.
        /// </summary>
        /// <param name="data">The data buffer containing serialized words.</param>
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

        /// <summary>
        /// Resets the reader's bit pointer to the beginning of the buffer.
        /// </summary>
        public void Reset()
        {
            _bitsRead = 0;
            _bitIndex = 0;
            _wordIndex = 0;
            _bitLength = _numBits;
            _scratch = Memory.NetworkToHost(_data[0]);
            _overflow = false;
        }

        /// <summary>
        /// Reads the specified number of bits from the buffer (max 32).
        /// </summary>
        /// <param name="bits">Number of bits to read (1 to 32).</param>
        /// <returns>An unsigned integer containing the read bits.</returns>
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

        /// <summary>
        /// Advances the bit pointer to align with the next byte boundary.
        /// </summary>
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

        /// <summary>
        /// Reads a block of bytes from the buffer directly into the provided span.
        /// Expects the reader to be byte-aligned.
        /// </summary>
        /// <param name="data">The destination span for the read bytes.</param>
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

        /// <summary>
        /// Gets the number of bits needed to align to the next byte boundary.
        /// </summary>
        public int GetAlignBits()
        {
            return (8 - _bitsRead % 8) % 8;
        }

        /// <summary>
        /// Gets the total number of bits read so far.
        /// </summary>
        public int GetBitsRead()
        {
            return _bitsRead;
        }
        
        private int GetBytesProcessed()
        {
            return _bitsRead / 8 + (_bitsRead % 8 > 0 ? 1 : 0);
        }

        /// <summary>
        /// Sets a custom limit on the total bit length to read.
        /// </summary>
        /// <param name="bitLength">The bit length threshold.</param>
        public void SetBitLength(int bitLength)
        {
            Assert.True(bitLength <= _numBits);
            _bitLength = bitLength;
        }

        /// <summary>
        /// Gets the current word boundary byte length read so far.
        /// </summary>
        public int GetBytesRead()
        {
            return (_wordIndex + 1) * 4;
        }

        /// <summary>
        /// Gets the number of remaining bits before hitting the end of the buffer.
        /// </summary>
        public int GetBitsRemaining()
        {
            return _numBits - _bitsRead;
        }

        /// <summary>
        /// Gets the total capacity of the reader in bits.
        /// </summary>
        public int GetTotalBits() 
        {
            return _numBits;
        }

        /// <summary>
        /// Gets the total capacity of the reader in bytes.
        /// </summary>
        public int GetTotalBytes()
        {
            return _numBits * 8;
        }

        /// <summary>
        /// Gets the underlying uint array backing this reader.
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
        /// Gets a value indicating whether a read operation overflowed the buffer capacity.
        /// </summary>
        public bool IsOverflow()
        {
            return _overflow;
        }
    }
}