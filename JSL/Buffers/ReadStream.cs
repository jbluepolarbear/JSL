// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.NetTypes;
using JSL.Utility;

namespace JSL.Buffers
{
    /// <summary>
    /// A pooled, reference-counted network read stream wrapping a <see cref="BitReader"/> to support rich unaligned type deserialization.
    /// </summary>
    public class ReadStream: NetRecyclable
    {
        /// <summary>
        /// Instantiates a new read stream with a default capacity of 2048 bytes.
        /// </summary>
        public ReadStream()
        {
            int size = 2048;
            _reader = new BitReader(size);
        }
        
        /// <summary>
        /// Instantiates a new read stream with the specified byte capacity.
        /// </summary>
        /// <param name="size">The capacity in bytes.</param>
        public ReadStream(int size)
        {
            _reader = new BitReader(size);
        }

        /// <summary>
        /// Resets the underlying reader to the beginning of the buffer.
        /// </summary>
        public void Reset()
        {
            _reader.Reset();
        }

        /// <summary>
        /// Fills the stream with a chunk of serialized byte data.
        /// </summary>
        /// <param name="data">The source data buffer.</param>
        /// <param name="offset">Start index offset in the data buffer.</param>
        /// <param name="size">Total size in bytes to copy.</param>
        public void Fill(ReadOnlySpan<byte> data, int offset, int size)
        {
            Memory.Copy(data, _reader.AsSpan(), offset, 0, size);
            _reader.Reset();
            _reader.SetBitLength(size * 8);
        }

        /// <summary>
        /// Fills the stream directly with the contents written to the provided write stream.
        /// </summary>
        /// <param name="writer">The source write stream.</param>
        public void Fill(WriteStream writer)
        {
            writer.MakeCopyable();
            var size = writer.GetBytesProcessed();
            Memory.Copy(writer.AsReadOnlySpan(), _reader.AsSpan(), 0, 0, size);
            _reader.Reset();
            _reader.SetBitLength(size * 32);
        }

        /// <summary>
        /// Reads an integer value that was serialized within a specific bounded range.
        /// </summary>
        /// <param name="min">The minimum bound.</param>
        /// <param name="max">The maximum bound.</param>
        /// <returns>The deserialized integer value.</returns>
        public int ReadIntRange(int min, int max)
        {
            Assert.True(min < max);
            var bits = Memory.BitsRequired(min, max);
            var value = (int) _reader.ReadBits(bits) + min;
            return value;
        }

        /// <summary>
        /// Reads the specified number of bits from the stream.
        /// </summary>
        /// <param name="bits">Number of bits to read (1 to 32).</param>
        /// <returns>An unsigned integer containing the read bits.</returns>
        public uint ReadBits(int bits)
        {
            Assert.True(bits > 0);
            Assert.True(bits <= 32);
            var value = _reader.ReadBits(bits);
            return value;
        }

        /// <summary>
        /// Aligns the stream pointer and reads a block of bytes into the target span.
        /// </summary>
        /// <param name="data">The destination span.</param>
        public void ReadBytes(Span<byte> data)
        {
            Align();
            _reader.ReadBytes(data);
        }
        
        /// <summary>
        /// Reads a single byte (8 bits) from the stream.
        /// </summary>
        /// <returns>The read byte.</returns>
        public byte ReadByte()
        {
            return (byte) _reader.ReadBits(8);
        }
        
        /// <summary>
        /// Reads a signed short (16 bits) from the stream.
        /// </summary>
        public short ReadInt16()
        {
            return (short) _reader.ReadBits(16);
        }
        
        /// <summary>
        /// Reads a signed int (32 bits) from the stream.
        /// </summary>
        public int ReadInt32()
        {
            return (int) _reader.ReadBits(32);
        }
        
        /// <summary>
        /// Reads a signed long (64 bits) from the stream.
        /// </summary>
        public long ReadInt64()
        {
            // low
            long value = _reader.ReadBits(32);
            // high
            value |= (long) _reader.ReadBits(32) << 32;
            return value;
        }
        
        /// <summary>
        /// Reads a char (16 bits) from the stream.
        /// </summary>
        public char ReadChar()
        {
            return (char) _reader.ReadBits(16);
        }
        
        /// <summary>
        /// Reads an unsigned short (16 bits) from the stream.
        /// </summary>
        public ushort ReadUInt16()
        {
            return (ushort) _reader.ReadBits(16);
        }
        
        /// <summary>
        /// Reads an unsigned int (32 bits) from the stream.
        /// </summary>
        public uint ReadUInt32()
        {
            return _reader.ReadBits(32);
        }
        
        /// <summary>
        /// Reads an unsigned long (64 bits) from the stream.
        /// </summary>
        public ulong ReadUInt64()
        {
            ulong value = _reader.ReadBits(32);
            value |= (ulong) _reader.ReadBits(32) << 32;
            return value;
        }

        /// <summary>
        /// Reads a single-precision float (32 bits) from the stream.
        /// </summary>
        public float ReadSingle()
        {
            return Memory.IntToFloat((int) _reader.ReadBits(32));
        }

        /// <summary>
        /// Reads a double-precision float (64 bits) from the stream.
        /// </summary>
        public double ReadDouble()
        {
            long value = _reader.ReadBits(32);
            value |= (long) _reader.ReadBits(32) << 32;
            return Memory.LongToDouble(value);
        }
        
        /// <summary>
        /// Advances the bit pointer to align with the next byte boundary.
        /// </summary>
        public void Align()
        {
            _reader.ReadAlign();
        }
        
        /// <summary>
        /// Gets the number of bits needed to align to the next byte boundary.
        /// </summary>
        public int ReadAligned()
        {
            return _reader.GetAlignBits();
        }
        
        /// <summary>
        /// Aligns the stream and validates the next 32 bits match the expected magic verification code.
        /// </summary>
        /// <param name="magic">The validation code to match.</param>
        /// <returns>True if the magic code matches; otherwise, false.</returns>
        public bool Check(uint magic)
        {
            Align();
            var value = ReadBits(32);
            return magic == value;
        }
        
        /// <summary>
        /// Gets the total bytes processed by the reader.
        /// </summary>
        public int ReadBytesProcessed()
        {
            var bitsRead = _reader.GetBitsRead();
            return bitsRead / 8 + (bitsRead % 8 != 0 ? 1 : 0);
        }

        /// <summary>
        /// Gets the total bits read from the stream.
        /// </summary>
        public int ReadBitsProcessed()
        {
            return _reader.GetBitsRead();
        }

        /// <summary>
        /// Sets a custom limit on the total bit length to read.
        /// </summary>
        /// <param name="bitLength">The bit length threshold.</param>
        public void SetBitLength(int bitLength)
        {
            _reader.SetBitLength(bitLength);
        }

        /// <summary>
        /// Gets the remaining bits that can be read before buffer overflow.
        /// </summary>
        public int ReadBitsRemaining()
        {
            return ReadTotalBits() - ReadBitsProcessed();
        }

        /// <summary>
        /// Gets the total bit capacity of the read stream.
        /// </summary>
        public int ReadTotalBits()
        {
            return _reader.GetTotalBytes() * 8;
        }

        /// <summary>
        /// Gets the total byte capacity of the read stream.
        /// </summary>
        public int ReadTotalBytes()
        {
            return _reader.GetTotalBytes();
        }

        /// <summary>
        /// Gets a value indicating whether a read operation caused a buffer overflow.
        /// </summary>
        public bool IsOverflow()
        {
            return _reader.IsOverflow();
        }

        private BitReader _reader;
        
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