using System;
using JSL.NetTypes;
using JSL.Utility;

namespace JSL.Buffers
{
    public class WriteStream: NetRecyclable
    {
        public WriteStream()
        {
            var size = 2048;
            _writer = new BitWriter(new uint[size / 4], size);
        }
        
        public WriteStream(int size)
        {
            _writer = new BitWriter(new uint[size / 4], size);
        }

        public void Reset()
        {
            _writer.Reset();
        }

        public int CopyBytes(byte[] data)
        {
            Align();
            var size = _writer.GetBytesWritten();
            Assert.True(data.Length >= size);
            Memory.Copy(_writer.GetData(), data, 0, 0, size);
            return size;
        }

        public void Write(INetRecyclable recyclable)
        {
            recyclable.Serialize(this);
        }

        public void WriteIntRange(int value, int min, int max)
        {
            Assert.True(min < max);
            Assert.True(value >= min);
            Assert.True(value <= max);
            var bits = Memory.BitsRequired(min, max);
            var unsignedValue = (uint) (value - min);
            _writer.WriteBits(unsignedValue, bits);
        }

        public void WriteBits(uint value, int bits)
        {
            Assert.True(bits > 0);
            Assert.True(bits <= 32);
            _writer.WriteBits(value, bits);
        }

        public void WriteBytes(byte[] data, int size)
        {
            if (data == null)
            {
                return;
            }
            Align();
            _writer.WriteBytes(data, size);
        }

        public void Write(byte value)
        {
            _writer.WriteBits(value, 8);
        }
        
        public void Write(short value)
        {
            _writer.WriteBits((uint) value, 16);
        }
        
        public void Write(int value)
        {
            _writer.WriteBits((uint) value, 32);
        }
        
        public void Write(long value)
        {
            // low
            _writer.WriteBits((uint) value, 32);
            // high
            _writer.WriteBits((uint) (value >> 32), 32);
        }
        
        public void Write(char value)
        {
            _writer.WriteBits((uint) value, 16);
        }
        
        public void Write(ushort value)
        {
            _writer.WriteBits((uint) value, 16);
        }
        
        public void Write(uint value)
        {
            _writer.WriteBits((uint) value, 32);
        }
        
        public void Write(ulong value)
        {
            // low
            _writer.WriteBits((uint) value, 32);
            // high
            _writer.WriteBits((uint) (value >> 32), 32);
        }
        
        public void Write(float value)
        {
            _writer.WriteBits((uint) Memory.FloatToInt(value), 32);
        }
        
        public void Write(double value)
        {
            var lvalue = Memory.DoubleToLong(value);
            // low
            _writer.WriteBits((uint) lvalue, 32);
            // high
            _writer.WriteBits((uint) (lvalue >> 32), 32);
        }
        
        // Add general write methods byte, short, int, long, float, etc
        
        public void Align()
        {
            _writer.WriteAlign();
        }
        
        public int GetAligned()
        {
            return _writer.GetAlignBits();
        }
        
        public bool Check(uint magic)
        {
            Align();
            WriteBits(magic, 32);
            return true;
        }

        public uint[] GetData()
        {
            return _writer.GetData();
        }
        
        public void Flush()
        {
            _writer.FlushBits();
        }

        public void SetBitPosition(int bitPosition)
        {
            _writer.SetBitPosition(bitPosition);
        }
        
        public int GetBytesProcessed()
        {
            return _writer.GetBytesWritten();
        }

        public int GetBitsProcessed()
        {
            return _writer.GetBitsWritten();
        }

        public int GetBitsRemaining()
        {
            return GetTotalBits() - GetBitsProcessed();
        }

        public int GetTotalBits()
        {
            return _writer.GetTotalBytes() * 8;
        }

        public int GetTotalBytes()
        {
            return _writer.GetTotalBytes();
        }

        public bool IsOverflow()
        {
            return _writer.IsOverflow();
        }

        private BitWriter _writer;
        
        protected override void AcquireImpl()
        {
            base.AcquireImpl();
            Reset();
        }

        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Reset();
        }

        public override void Serialize(WriteStream writer)
        {
            throw new NotImplementedException();
        }

        public override void Deserialize(ReadStream reader)
        {
            throw new NotImplementedException();
        }
    }
}