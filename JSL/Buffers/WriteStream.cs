// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.NetTypes;
using JSL.Pooled;
using JSL.Utility;

namespace JSL.Buffers
{
    /// <summary>
    /// A pooled, reference-counted network write stream wrapping a <see cref="BitWriter"/> to support rich unaligned type serialization.
    /// </summary>
    public class WriteStream: NetRecyclable
    {
        /// <summary>
        /// Instantiates a new write stream with a default capacity of 2048 bytes.
        /// </summary>
        public WriteStream()
        {
            var size = 2048;
            _writer = new BitWriter(size);
        }
        
        /// <summary>
        /// Instantiates a new write stream with the specified byte capacity.
        /// </summary>
        /// <param name="size">The capacity in bytes.</param>
        public WriteStream(int size)
        {
            _writer = new BitWriter(size);
        }

        /// <summary>
        /// Resets the underlying writer to the beginning of the buffer.
        /// </summary>
        public void Reset()
        {
            _writer.Reset();
        }

        /// <summary>
        /// Flushes and aligns the write stream, then copies the serialized data into the target byte span.
        /// </summary>
        /// <param name="data">The destination span.</param>
        /// <returns>The number of bytes written.</returns>
        public int CopyBytes(Span<byte> data)
        {
            MakeCopyable();
            var size = _writer.GetBytesWritten();
            Assert.True(data.Length >= size);
            Memory.Copy(_writer.AsReadOnlySpan(), data, 0, 0, size);
            return size;
        }

        /// <summary>
        /// Writes an integer value within a specific bounded range using only the minimum required number of bits.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="min">The minimum bound.</param>
        /// <param name="max">The maximum bound.</param>
        public void WriteIntRange(int value, int min, int max)
        {
            Assert.True(min < max);
            Assert.True(value >= min);
            Assert.True(value <= max);
            var bits = Memory.BitsRequired(min, max);
            var unsignedValue = (uint) (value - min);
            _writer.WriteBits(unsignedValue, bits);
        }

        /// <summary>
        /// Writes the specified number of bits to the stream.
        /// </summary>
        /// <param name="value">The unsigned integer containing the bits.</param>
        /// <param name="bits">Number of bits to write (1 to 32).</param>
        public void WriteBits(uint value, int bits)
        {
            Assert.True(bits > 0);
            Assert.True(bits <= 32);
            _writer.WriteBits(value, bits);
        }

        /// <summary>
        /// Align bit pointer to next byte boundary and writes the specified byte array buffer.
        /// </summary>
        /// <param name="data">The byte array payload.</param>
        /// <param name="size">The size to write in bytes.</param>
        public void WriteBytes(byte[] data, int size)
        {
            if (data == null)
            {
                return;
            }
            Align();
            _writer.WriteBytes(data, size);
        }

        /// <summary>
        /// Writes a single byte (8 bits) to the stream.
        /// </summary>
        public void Write(byte value)
        {
            _writer.WriteBits(value, 8);
        }
        
        /// <summary>
        /// Writes a signed short (16 bits) to the stream.
        /// </summary>
        public void Write(short value)
        {
            _writer.WriteBits((uint) value, 16);
        }
        
        /// <summary>
        /// Writes a signed int (32 bits) to the stream.
        /// </summary>
        public void Write(int value)
        {
            _writer.WriteBits((uint) value, 32);
        }
        
        /// <summary>
        /// Writes a signed long (64 bits) to the stream.
        /// </summary>
        public void Write(long value)
        {
            // low
            _writer.WriteBits((uint) value, 32);
            // high
            _writer.WriteBits((uint) (value >> 32), 32);
        }
        
        /// <summary>
        /// Writes a char (16 bits) to the stream.
        /// </summary>
        public void Write(char value)
        {
            _writer.WriteBits((uint) value, 16);
        }
        
        /// <summary>
        /// Writes an unsigned short (16 bits) to the stream.
        /// </summary>
        public void Write(ushort value)
        {
            _writer.WriteBits((uint) value, 16);
        }
        
        /// <summary>
        /// Writes an unsigned int (32 bits) to the stream.
        /// </summary>
        public void Write(uint value)
        {
            _writer.WriteBits((uint) value, 32);
        }
        
        /// <summary>
        /// Writes an unsigned long (64 bits) to the stream.
        /// </summary>
        public void Write(ulong value)
        {
            // low
            _writer.WriteBits((uint) value, 32);
            // high
            _writer.WriteBits((uint) (value >> 32), 32);
        }
        
        /// <summary>
        /// Writes a single-precision float (32 bits) to the stream.
        /// </summary>
        public void Write(float value)
        {
            _writer.WriteBits((uint) Memory.FloatToInt(value), 32);
        }
        
        /// <summary>
        /// Writes a double-precision float (64 bits) to the stream.
        /// </summary>
        public void Write(double value)
        {
            var lvalue = Memory.DoubleToLong(value);
            // low
            _writer.WriteBits((uint) lvalue, 32);
            // high
            _writer.WriteBits((uint) (lvalue >> 32), 32);
        }
        
        /// <summary>
        /// Pads with zero bits to align the bit pointer to the next byte boundary.
        /// </summary>
        public void Align()
        {
            _writer.WriteAlign();
        }
        
        /// <summary>
        /// Gets the number of bits needed to align to the next byte boundary.
        /// </summary>
        public int GetAligned()
        {
            return _writer.GetAlignBits();
        }
        
        /// <summary>
        /// Aligns the stream and writes a 32-bit validation magic number.
        /// </summary>
        /// <param name="magic">The validation code.</param>
        /// <returns>True on success.</returns>
        public bool Check(uint magic)
        {
            Align();
            WriteBits(magic, 32);
            return true;
        }

        /// <summary>
        /// Gets the underlying pooled uint buffer array.
        /// </summary>
        public Array<uint> GetData()
        {
            return _writer.GetData();
        }
        
        /// <summary>
        /// Returns the underlying buffer as a span of uint words.
        /// </summary>
        public Span<uint> AsSpan()
        {
            return _writer.AsSpan();
        }
        
        /// <summary>
        /// Returns the underlying buffer as a read-only span of uint words.
        /// </summary>
        public ReadOnlySpan<uint> AsReadOnlySpan()
        {
            return _writer.AsReadOnlySpan();
        }
        
        /// <summary>
        /// Flushes the bit writer, pushing any remaining scratch-pad bits into the word buffer.
        /// </summary>
        public void Flush()
        {
            _writer.FlushBits();
        }

        /// <summary>
        /// Aligns and flushes the write stream to make it ready for copying or transmission.
        /// </summary>
        public void MakeCopyable()
        {
            Align();
            Flush();
        }

        /// <summary>
        /// Sets the write stream bit position.
        /// </summary>
        /// <param name="bitPosition">The new bit index pointer.</param>
        public void SetBitPosition(int bitPosition)
        {
            _writer.SetBitPosition(bitPosition);
        }
        
        /// <summary>
        /// Gets the total bytes processed by the stream.
        /// </summary>
        public int GetBytesProcessed()
        {
            return _writer.GetBytesWritten();
        }

        /// <summary>
        /// Gets the total bits written to the stream.
        /// </summary>
        public int GetBitsProcessed()
        {
            return _writer.GetBitsWritten();
        }

        /// <summary>
        /// Gets the remaining bits that can be written before buffer overflow.
        /// </summary>
        public int GetBitsRemaining()
        {
            return GetTotalBits() - GetBitsProcessed();
        }

        /// <summary>
        /// Gets the total bit capacity of the write stream.
        /// </summary>
        public int GetTotalBits()
        {
            return _writer.GetTotalBytes() * 8;
        }

        /// <summary>
        /// Gets the total byte capacity of the write stream.
        /// </summary>
        public int GetTotalBytes()
        {
            return _writer.GetTotalBytes();
        }

        /// <summary>
        /// Gets a value indicating whether a write operation caused a buffer overflow.
        /// </summary>
        public bool IsOverflow()
        {
            return _writer.IsOverflow();
        }

        private BitWriter _writer;
        
        /// <summary>
        /// Overridden acquire callback that resets the stream state.
        /// </summary>
        protected override void AcquireImpl()
        {
            base.AcquireImpl();
            Reset();
        }

        /// <summary>
        /// Overridden release callback that resets the stream state.
        /// </summary>
        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Reset();
        }
    }
}