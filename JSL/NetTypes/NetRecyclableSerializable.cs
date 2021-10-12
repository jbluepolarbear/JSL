using JSL.Buffers;

namespace JSL.NetTypes
{
    public abstract class NetRecyclableSerializable: NetRecyclable, INetSerializable
    {
        public abstract void Serialize(WriteStream writer);

        public abstract void Deserialize(ReadStream reader);
    }
}