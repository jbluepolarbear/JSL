using JSL.Buffers;

namespace JSL.NetTypes
{
    public class NetPlayerInfo: NetRecyclable
    {
        public NetId Id;
        public NetString Name;
        public override void Serialize(WriteStream writer)
        {
            Id.Serialize(writer);
            Name.Serialize(writer);
        }

        public override void Deserialize(ReadStream reader)
        {
            Id.Deserialize(reader);
            Name.Deserialize(reader);
        }

        protected override void AcquireImpl()
        {
            base.AcquireImpl();
            Id = MemoryManager.RecyclablePool.Get<NetId>();
            Name = MemoryManager.RecyclablePool.Get<NetString>();
        }

        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Id.Dispose();
            Id = null;
            Name.Dispose();
            Name = null;
        }
    }
}