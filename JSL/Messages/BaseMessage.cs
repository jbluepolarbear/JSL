using JSL.Buffers;
using JSL.NetTypes;

namespace JSL.Messages
{
    public abstract partial class BaseMessage: NetRecyclableSerializable
    {
        public abstract uint TypeId { get; }
        public uint Id;
        public uint FromId; // Client or server that sent message
        public uint ToId; // 0 means every connection
        public uint Frame; // current game frame time is calculated from gamestart message time at fixed rate interval 
        public override void Serialize(WriteStream writer)
        {
            writer.Write((uint) Id);
            writer.Write((uint) FromId);
            writer.Write((uint) ToId);
            writer.Write((uint) Frame);
        }

        public override void Deserialize(ReadStream reader)
        {
            Id = reader.ReadUInt32();
            FromId = reader.ReadUInt32();
            ToId = reader.ReadUInt32();
            Frame = reader.ReadUInt32();
        }

        protected override void AcquireImpl()
        {
            base.AcquireImpl();
        }

        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Id = 0;
            FromId = 0;
            ToId = 0;
            Frame = 0;
        }

        public static implicit operator string(BaseMessage netString) => netString.GetType().ToString();
    }
}