// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.


using System;
using JSL.Pools;
using JSL.Messages;

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
TestTypeTrio.ClassId => new TestTypeTrio(),

            _ => throw new ArgumentOutOfRangeException($"Type Id: {typeId} not found.")
        };
    }
}
