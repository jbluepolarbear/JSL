// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.NetTypes;
using JSL.Buffers;
using JSL.Pools;
using NUnit.Framework;

namespace JSLTest
{
    [TestFixture]
    public class NetStringTest
    {
        [SetUp]
        public void Setup()
        {
            MemoryManager.Instance.Clear();
        }

        [Test]
        public void TestNetStringThreadSafety()
        {
            var threads = new System.Threading.Thread[10];
            var success = true;

            for (int t = 0; t < threads.Length; ++t)
            {
                var threadId = t;
                threads[t] = new System.Threading.Thread(() =>
                {
                    try
                    {
                        var expectedString = $"ThreadString-{threadId}";
                        using var writer = new WriteStream();
                        using var netStr = new NetString(expectedString);
                        netStr.Serialize(writer);

                        var buffer = new byte[512];
                        var size = writer.CopyBytes(buffer);

                        for (int i = 0; i < 100; ++i)
                        {
                            using var reader = new ReadStream();
                            reader.Fill(buffer, 0, size);

                            using var netStrRead = new NetString();
                            netStrRead.Deserialize(reader);

                            if (netStrRead.String != expectedString)
                            {
                                success = false;
                            }
                        }
                    }
                    catch
                    {
                        success = false;
                    }
                });
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.IsTrue(success);
        }
    }
}
