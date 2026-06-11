// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.


using JSL.Buffers;
using JSL.NetTypes;
using JSL.Pools;
using JSL.Messages;

namespace JSLTest.Messages.Generated
{
    public class TestTypeDuo: BaseMessage
    {       
        public const uint ClassId = 2;
        public override uint TypeId => 2;
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
TestField = MemoryManager.RecyclablePool.Get<JSL.NetTypes.NetCompressedVector3>();

        }
        
        protected override void ReleaseImpl()
        {
// Release
base.ReleaseImpl();
TestField.Dispose();
TestField = null;

        }
        
public JSL.NetTypes.NetCompressedVector3 TestField;

    }
}
