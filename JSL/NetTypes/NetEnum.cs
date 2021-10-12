using System;
using JSL.Buffers;

namespace JSL.NetTypes
{
    /// <summary>
    /// Enum wrapper
    /// </summary>
    public class NetEnum: NetRecyclableSerializable
    {
        public NetEnum()
        {
            
        }
        
        public byte _value;
        
        public override void Serialize(WriteStream writer)
        {
            writer.Write(_value);
        }

        public override void Deserialize(ReadStream reader)
        {
            _value = reader.ReadByte();
        }
    }

    public class NetEnum<T> : NetEnum where T: Enum
    {
        public T Value
        {
            get => (T) (object) _value;
            set => _value = (byte) (object) value;
        }
    }
}