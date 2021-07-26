
using JSL.Buffers;
using JSL.NetTypes;
using JSL.Pools;
using JSL.Messages;

namespace JSLTest.Messages.Generated
{
    public class TestType: BaseMessage
    {       
        public const uint ClassId = 1;
        public override uint TypeId => 1;
        public override void Serialize(WriteStream writer)
        {
// Writer
base.Serialize(writer);
TestField.Serialize(writer);

        }

        public override void Deserialize(ReadStream reader)
        {
// Reader
base.Deserialize(reader);
TestField.Deserialize(reader);

        }

        protected override void AcquireImpl()
        {
// Acquire
base.AcquireImpl();
TestField = MemoryManager.RecyclablePool.Get<JSL.NetTypes.NetQuat>();

        }
        
        protected override void ReleaseImpl()
        {
// Release
base.ReleaseImpl();
TestField.Dispose();
TestField = null;

        }
        
public JSL.NetTypes.NetQuat TestField;

    }
}
