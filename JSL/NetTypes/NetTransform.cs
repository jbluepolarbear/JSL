using JSL.Buffers;

namespace JSL.NetTypes
{
    public class NetTransform: NetRecyclable
    {
        public override void Serialize(WriteStream writer)
        {
            Id.Serialize(writer);
            if (Id.Active)
            {
                Position.Serialize(writer);
                Rotation.Serialize(writer);
            }
        }

        public override void Deserialize(ReadStream reader)
        {
            Id.Deserialize(reader);
            if (Id.Active)
            {
                Position.Deserialize(reader);
                Rotation.Deserialize(reader);
            }
        }

        protected override void AcquireImpl()
        {
            base.AcquireImpl();
            Id = MemoryManager.RecyclablePool.Get<NetId>();
            Position = MemoryManager.RecyclablePool.Get<NetVector3>();
            Rotation = MemoryManager.RecyclablePool.Get<NetQuat>();
        }

        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Id.Dispose();
            Id = null;
            Position.Dispose();
            Position = null;
            Rotation.Dispose();
            Rotation = null;
        }

        public NetId Id; // 16 bits
        public NetVector3 Position; // 56 bits
        public NetQuat Rotation; // 32 bits
        // Max Entities that will fit in a single data frame message when baseline.
        public const int MaxEntities = 78;
    }
}