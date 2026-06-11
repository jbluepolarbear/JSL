// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.NetTypes;
using JSL.Pools;
using JSL.Buffers;
using NUnit.Framework;

namespace JSLTest
{
    [TestFixture]
    public class NetUnitFloatTest
    {
        [SetUp]
        public void Setup()
        {
            MemoryManager.Instance.Clear();
        }

        [Test]
        public void TestNetUnitFloatRange()
        {
            var values = new float[] { -1.0f, -0.5f, 0.0f, 0.5f, 1.0f };
            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
            
            foreach (var val in values)
            {
                using var netFloat = MemoryManager.Instance.RecyclablePool.Get<NetUnitFloat>();
                netFloat.Value = val;
                netFloat.Serialize(writer);
            }

            var copyBuffer = new byte[1024];
            var size = writer.CopyBytes(copyBuffer);

            using var reader = MemoryManager.Instance.RecyclablePool.Get<ReadStream>();
            reader.Fill(copyBuffer, 0, size);

            foreach (var expectedVal in values)
            {
                using var netFloat = MemoryManager.Instance.RecyclablePool.Get<NetUnitFloat>();
                netFloat.Deserialize(reader);
                Assert.That(netFloat.Value, Is.EqualTo(expectedVal).Within(0.0001f));
            }
        }
    }
}
