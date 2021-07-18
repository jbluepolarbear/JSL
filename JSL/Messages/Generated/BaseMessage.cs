using System;

namespace JSL.Messages
{
    public abstract partial class BaseMessage
    {        
        public static implicit operator Messages.BaseMessage(uint typeId) => typeId switch
        {
// type construction

_ => throw new ArgumentOutOfRangeException($"Type Id: {typeId} not found.")
        };
    }
}