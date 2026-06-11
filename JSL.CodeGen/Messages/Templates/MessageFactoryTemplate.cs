// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
    public static class MessageFactory
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