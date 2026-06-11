// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.NetTypes;
using JSL.Pools;
using NUnit.Framework;

namespace JSLTest
{
    [TestFixture]
    public class NetBasePoolTest
    {
        [SetUp]
        public void Setup()
        {
            MemoryManager.Instance.Clear();
        }

        [Test]
        public void TestNetBasePoolO1Performance()
        {
            MemoryManager.Instance.Clear();

            // Verify that we retrieve the exact same instance immediately when disposed
            NetId instance1;
            using (var temp = MemoryManager.Instance.RecyclablePool.Get<NetId>())
            {
                instance1 = temp;
                Assert.AreEqual(1, instance1.RefCount);
            } // RefCount drops to 0, item enqueued back to pool queue

            using (var temp2 = MemoryManager.Instance.RecyclablePool.Get<NetId>())
            {
                // Should return the exact same instance
                Assert.AreSame(instance1, temp2);
                Assert.AreEqual(1, temp2.RefCount);
            }
        }
    }
}
