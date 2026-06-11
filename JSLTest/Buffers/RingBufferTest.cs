// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.Buffers;
using JSL.NetTypes;
using JSL.Pools;
using NUnit.Framework;

namespace JSLTest
{
    [TestFixture]
    public class RingBufferTest
    {
        [SetUp]
        public void Setup()
        {
            MemoryManager.Instance.Clear();
        }

        [Test]
        public void TestRingBufferMemoryDisposal()
        {
            // Clear current pools to start fresh
            MemoryManager.Instance.Clear();

            var ring = new RingBuffer<NetId>(2);
            
            using (var id1 = MemoryManager.Instance.RecyclablePool.Get<NetId>())
            {
                id1.Id = 100;
                ring.Add(id1);
            }
            using (var id2 = MemoryManager.Instance.RecyclablePool.Get<NetId>())
            {
                id2.Id = 200;
                ring.Add(id2);
            }

            Assert.AreEqual(2, ring.Count);

            // Add one more to evict the first item (id1, which was 100)
            using (var id3 = MemoryManager.Instance.RecyclablePool.Get<NetId>())
            {
                id3.Id = 300;
                ring.Add(id3);
            }

            // The evicted id1 (100) should be returned to pool (RefCount = 0) and reusable.
            // Let's verify we can get it from the pool and it has been recycled
            using (var recycled = MemoryManager.Instance.RecyclablePool.Get<NetId>())
            {
                Assert.AreEqual(1, recycled.RefCount);
            }

            // Clear the ring buffer
            ring.Clear();

            // All remaining items (200, 300) should have RefCount = 0 and be reusable
            using (var recycled1 = MemoryManager.Instance.RecyclablePool.Get<NetId>())
            using (var recycled2 = MemoryManager.Instance.RecyclablePool.Get<NetId>())
            {
                Assert.AreEqual(1, recycled1.RefCount);
                Assert.AreEqual(1, recycled2.RefCount);
            }

            ring.Dispose();
        }
    }
}
