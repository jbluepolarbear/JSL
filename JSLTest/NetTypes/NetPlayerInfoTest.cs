// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.NetTypes;
using JSL.Buffers;
using JSL.Pools;
using NUnit.Framework;

namespace JSLTest
{
    [TestFixture]
    public class NetPlayerInfoTest
    {
        [SetUp]
        public void Setup()
        {
            MemoryManager.Instance.Clear();
        }

        [Test]
        public void TestNetPlayerInfo()
        {
            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
            using var playerWrite = MemoryManager.Instance.RecyclablePool.Get<NetPlayerInfo>();
            
            playerWrite.Id.Id = 42;
            playerWrite.Id.Active = true;
            playerWrite.Name.String = "PlayerOne";
            playerWrite.Serialize(writer);

            var buffer = new byte[256];
            var size = writer.CopyBytes(buffer);

            using var reader = MemoryManager.Instance.RecyclablePool.Get<ReadStream>();
            reader.Fill(buffer, 0, size);

            using var playerRead = MemoryManager.Instance.RecyclablePool.Get<NetPlayerInfo>();
            playerRead.Deserialize(reader);

            Assert.AreEqual(42, playerRead.Id.Id);
            Assert.IsTrue(playerRead.Id.Active);
            Assert.AreEqual("PlayerOne", playerRead.Name.String);
        }
    }
}
