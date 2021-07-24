using System;

namespace JSL.Messages.Generator
{
    [Serializable]
    public class MessageType
    {
        public string Name;
        public string Base;
        public MessageData[] Data;
    }
}
