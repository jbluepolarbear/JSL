using System.Collections.Generic;
using JSL.Messages.Generator;

namespace JSL.CodeGen.Messages.Generator
{
    public class MessageGeneratorConfig
    {
        public string OutputNamespace;
        public string OutputPath;
        public IEnumerable<MessageType> MessageTypes;
    }
}