using JSL.Buffers;

namespace JSL.NetTypes
{
    public interface INetSerializable
    {
        void Serialize(WriteStream writer);
        void Deserialize(ReadStream reader);
    }
}