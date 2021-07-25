namespace JSL.CodeGen.Messages.Templates
{
    public static class MessageFactoryTemplate
    {
        public const string Tempalte = @"
using System;
using JSL.Pools;
using JSL.Messages;

namespace [[NAMESPACE]]
{
    public abstract class MessageFactory
    {
        public static void RegisterMessageFactory()
        {
            MemoryManager.Instance.GeneratedMessagePool.RegisterMessageFactory(FromType);
        }
        
        public static BaseMessage FromType(uint typeId) => typeId switch
        {
[[MESSAGE_CONSTRUCTORS]]
            _ => throw new ArgumentOutOfRangeException($""Type Id: {typeId} not found."")
        };
    }
}
";
    }
}