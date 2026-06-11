// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.NetTypes;
using JSL.Buffers;
using JSL.Pools;
using NUnit.Framework;

namespace JSLTest
{
    [TestFixture]
    public class NetVector2Test
    {
        [SetUp]
        public void Setup()
        {
            MemoryManager.Instance.Clear();
        }

        [Test]
        public void TestNetVector2Compression()
        {
            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
            using var vecWrite = MemoryManager.Instance.RecyclablePool.Get<NetVector2>();
            
            vecWrite.X = -131072.0f;
            vecWrite.Y = 131071.0f;
            vecWrite.Serialize(writer);

            var buffer = new byte[1024];
            var size = writer.CopyBytes(buffer);

            using var reader = MemoryManager.Instance.RecyclablePool.Get<ReadStream>();
            reader.Fill(buffer, 0, size);

            using var vecRead = MemoryManager.Instance.RecyclablePool.Get<NetVector2>();
            vecRead.Deserialize(reader);

            Assert.That(vecRead.X, Is.EqualTo(vecWrite.X).Within(1.0f));
            Assert.That(vecRead.Y, Is.EqualTo(vecWrite.Y).Within(1.0f));
        }
    }
}
