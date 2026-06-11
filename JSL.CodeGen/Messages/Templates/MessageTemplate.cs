// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace JSL.CodeGen.Messages.Templates
{
    public static class MessageTemplate
    {
        public const string Template = @"
using JSL.Buffers;
using JSL.NetTypes;
using JSL.Pools;
using JSL.Messages;

namespace [[NAMESPACE]]
{
    public class [[CLASS_NAME]]: [[BASE_CLASS]]
    {       
        public const uint ClassId = [[TYPE_ID]];
        public override uint TypeId => [[TYPE_ID]];
        public override void Serialize(WriteStream writer)
        {
// Writer
base.Serialize(writer);
[[WRITE_MEMBERS]]
        }

        public override void Deserialize(ReadStream reader)
        {
// Reader
base.Deserialize(reader);
[[READ_MEMBERS]]
        }

        protected override void AcquireImpl()
        {
// Acquire
base.AcquireImpl();
[[ACQUIRE_MEMBERS]]
        }
        
        protected override void ReleaseImpl()
        {
// Release
base.ReleaseImpl();
[[RELEASE_MEMBERS]]
        }
        
[[MEMBERS]]
    }
}
";
    }
}