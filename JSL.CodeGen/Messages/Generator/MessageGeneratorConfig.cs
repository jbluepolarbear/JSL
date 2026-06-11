// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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