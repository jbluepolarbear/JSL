using JSL.Buffers;

namespace JSL.NetTypes
{
    public class NetId: NetRecyclableSerializable
    {
        public override void Serialize(WriteStream writer)
        {
            writer.WriteBits(Id, 15);
            writer.WriteBits(Active ? 1u : 0u, 1);
        }

        public override void Deserialize(ReadStream reader)
        {
            Id = (ushort) reader.ReadBits(15);
            Active = reader.ReadBits(1) == 1;
        }

        public ushort Id; // only Ids up to 2^15. first bit is for active state
        public bool Active;
        
        public static implicit operator ushort(NetId netId) => netId.Id;
        public static implicit operator NetId(ushort id) => new NetId{Id = id, Active = true};
    }
}