using JSL.NetTypes;
using JSL.Utility;

namespace JSL.Buffers
{
    public class ReadStream: NetRecyclable
    {
        public ReadStream()
        {
            int size = 2048;
            _reader = new BitReader(new uint[size / 4], size);
        }
        
        public ReadStream(int size)
        {
            _reader = new BitReader(new uint[size / 4], size);
        }

        public void Reset()
        {
            _reader.Reset();
        }

        public void Fill(byte[] data, int offset, int size)
        {
            Memory.Copy(data, _reader.GetData(), offset, 0, size);
            _reader.Reset();
            _reader.SetBitLength(size * 8);
        }

        public void Fill(WriteStream writer)
        {
            writer.MakeCopyable();
            var size = writer.GetBytesProcessed();
            Memory.Copy(writer.GetData(), _reader.GetData(), 0, 0, size);
            _reader.Reset();
            _reader.SetBitLength(size * 32);
        }

        public void Read(INetRecyclable recyclable)
        {
            recyclable.Deserialize(this);
        }

        public int ReadIntRange(int min, int max)
        {
            Assert.True(min < max);
            var bits = Memory.BitsRequired(min, max);
            var value = (int) _reader.ReadBits(bits) + min;
            return value;
        }

        public uint ReadBits(int bits)
        {
            Assert.True(bits > 0);
            Assert.True(bits <= 32);
            var value = _reader.ReadBits(bits);
            return value;
        }

        public void ReadBytes(byte[] data, int size)
        {
            if (data == null)
            {
                return;
            }
            Align();
            _reader.ReadBytes(data, size);
        }
        
        public byte ReadByte()
        {
            return (byte) _reader.ReadBits(8);
        }
        
        public short ReadInt16()
        {
            return (short) _reader.ReadBits(16);
        }
        
        public int ReadInt32()
        {
            return (int) _reader.ReadBits(32);
        }
        
        public long ReadInt64()
        {
            // low
            long value = _reader.ReadBits(32);
            // high
            value |= (long) _reader.ReadBits(32) << 32;
            return value;
        }
        
        public char ReadChar()
        {
            return (char) _reader.ReadBits(16);
        }
        
        public ushort ReadUInt16()
        {
            return (ushort) _reader.ReadBits(16);
        }
        
        public uint ReadUInt32()
        {
            return _reader.ReadBits(32);
        }
        
        public ulong ReadUInt64()
        {
            ulong value = _reader.ReadBits(32);
            value |= (ulong) _reader.ReadBits(32) << 32;
            return value;
        }

        public float ReadSingle()
        {
            return Memory.IntToFloat((int) _reader.ReadBits(32));
        }

        public double ReadDouble()
        {
            long value = _reader.ReadBits(32);
            value |= (long) _reader.ReadBits(32) << 32;
            return Memory.LongToDouble(value);
        }
        
        public void Align()
        {
            _reader.ReadAlign();
        }
        
        public int ReadAligned()
        {
            return _reader.GetAlignBits();
        }
        
        public bool Check(uint magic)
        {
            Align();
            var value = ReadBits(32);
            return magic == value;
        }
        
        public int ReadBytesProcessed()
        {
            var bitsRead = _reader.GetBitsRead();
            return bitsRead / 8 + (bitsRead % 8 != 0 ? 1 : 0);
        }

        public int ReadBitsProcessed()
        {
            return _reader.GetBitsRead();
        }

        public void SetBitLength(int bitLength)
        {
            _reader.SetBitLength(bitLength);
        }

        public int ReadBitsRemaining()
        {
            return ReadTotalBits() - ReadBitsProcessed();
        }

        public int ReadTotalBits()
        {
            return _reader.GetTotalBytes() * 8;
        }

        public int ReadTotalBytes()
        {
            return _reader.GetTotalBytes();
        }

        public bool IsOverflow()
        {
            return _reader.IsOverflow();
        }

        private BitReader _reader;
        
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
            throw new System.NotImplementedException();
        }

        public override void Deserialize(ReadStream reader)
        {
            throw new System.NotImplementedException();
        }
    }
}