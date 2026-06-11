// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.NetTypes;
using JSL.Buffers;
using JSL.Pools;
using NUnit.Framework;

namespace JSLTest
{
    [TestFixture]
    public class NetEnumTest
    {
        private enum TestByteEnum : byte
        {
            First = 0,
            Second = 1,
            Last = 255
        }

        [SetUp]
        public void Setup()
        {
            MemoryManager.Instance.Clear();
        }

        [Test]
        public void TestNetEnum()
        {
            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
            using var enumWrite = MemoryManager.Instance.RecyclablePool.Get<NetEnum<TestByteEnum>>();
            
            enumWrite.Value = TestByteEnum.Last;
            enumWrite.Serialize(writer);

            var buffer = new byte[128];
            var size = writer.CopyBytes(buffer);

            using var reader = MemoryManager.Instance.RecyclablePool.Get<ReadStream>();
            reader.Fill(buffer, 0, size);

            using var enumRead = MemoryManager.Instance.RecyclablePool.Get<NetEnum<TestByteEnum>>();
            enumRead.Deserialize(reader);

            Assert.AreEqual(TestByteEnum.Last, enumRead.Value);
        }
    }
}
