using System;
using JSL.Buffers;
using JSL.Messages;

namespace JSL.NetTypes
{
    /// <summary>
    /// Allow Generated Messages to Serialize and Deserialize without Reflection
    /// Each Generated Type has TypeId and
    /// a BaseMessage partial class is generated with a switch implicit constructor on the TypeId
    /// </summary>
    public class NetMessage: NetRecyclableSerializable
    {
        public BaseMessage Message;

        public override void Serialize(WriteStream writer)
        {
            if (Message == null)
            {
                throw new NullReferenceException("Message can't be null");
            }
            writer.Write(Message.TypeId);
            Message.Serialize(writer);
        }

        public override void Deserialize(ReadStream reader)
        {
            var typeId = reader.ReadUInt32();
            // Fix this.
            Message = MemoryManager.GeneratedMessagePool.Get(typeId);
            Message.Deserialize(reader);
        }

        protected override void AcquireImpl()
        {
            base.AcquireImpl();
        }

        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Message?.Dispose();
            Message = null;
        }
    }
}