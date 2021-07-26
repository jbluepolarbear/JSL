
using System;
using JSL.Pools;
using JSL.Messages;
using NetLib.Messages.Generated;

namespace JSLTest.Messages.Generated
{
    public static class MessageFactory
    {
        public static void RegisterMessageFactory()
        {
            MemoryManager.Instance.GeneratedMessagePool.RegisterMessageFactory(FromType);
        }
        
        public static BaseMessage FromType(uint typeId) => typeId switch
        {
TestType.ClassId => new TestType(),
TestTypeDuo.ClassId => new TestTypeDuo(),

            _ => throw new ArgumentOutOfRangeException($"Type Id: {typeId} not found.")
        };
    }
}
