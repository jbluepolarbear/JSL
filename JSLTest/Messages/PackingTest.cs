// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JSL.Buffers;
using JSL.CodeGen.Messages.Generator;
using JSL.Messages.Generator;
using JSL.Messages.Util;
using JSL.NetTypes;
using JSL.Pools;
using JSLTest.Messages.Generated;
using NUnit.Framework;

namespace JSLTest
{
    [TestFixture]
    public class PackingTest
    {
        [SetUp]
        public void Setup()
        {
            MemoryManager.Instance.Clear();
            MessageFactory.RegisterMessageFactory();
            //
            // Example of using the code gen.
            // The data definitions could be defined in code or from a json or xml file.
            //
            
            // string outputPath = @$"{Directory.GetCurrentDirectory()}/../../../Messages/Generated/";
            // var config = new MessageGeneratorConfig
            // {
            //     OutputNamespace = "JSLTest.Messages.Generated",
            //     OutputPath = outputPath,
            //     MessageTypes = new []
            //     {
            //         new MessageType
            //         {
            //             Name = "TestType",
            //             Data = new []
            //             {
            //                 new MessageData
            //                 {
            //                     Name = "TestField",
            //                     Type = "NetQuat"
            //                 }
            //             }
            //         },
            //         new MessageType
            //         {
            //             Name = "TestTypeDuo",
            //             Data = new []
            //             {
            //                 new MessageData
            //                 {
            //                     Name = "TestField",
            //                     Type = "NetCompressedVector3"
            //                 }
            //             }
            //         },
            //         new MessageType
            //         {
            //             Name = "TestTypeTrio",
            //             Data = new []
            //             {
            //                 new MessageData
            //                 {
            //                     Name = "TestField",
            //                     Type = "NetVector3"
            //                 }
            //             }
            //         }
            //     }
            // };
            // new MessageGenerator().Generate(config);
        }

        [Test]
        public void Packing()
        {
            var tolerance = 0.02f;
            var queue = new Queue<NetMessage>();
            using var netMsg = MemoryManager.Instance.MessagePool.Get(TestType.ClassId);
            var testType = (TestType) netMsg.Message;
            testType.TestField.X = 0.0f;
            testType.TestField.Y = 0.0f;
            testType.TestField.Z = 0.0f;
            testType.TestField.W = 1.0f;
            netMsg.Acquire();
            queue.Enqueue(netMsg);
            
            using var netMsgDuo = MemoryManager.Instance.MessagePool.Get(TestTypeDuo.ClassId);
            var testTypeDuo = (TestTypeDuo) netMsgDuo.Message;
            testTypeDuo.TestField.X = 123.5f;
            testTypeDuo.TestField.Y = 2.23f;
            testTypeDuo.TestField.Z = 189.23f;
            netMsgDuo.Acquire();
            queue.Enqueue(netMsgDuo);
            
            using var netMsgTrio = MemoryManager.Instance.MessagePool.Get(TestTypeTrio.ClassId);
            var testTypeTrio = (TestTypeTrio) netMsgTrio.Message;
            testTypeTrio.TestField.X = 0.707106829f;
            testTypeTrio.TestField.Y = 0.0f;
            testTypeTrio.TestField.Z = 0.707106829f;
            netMsgTrio.Acquire();
            queue.Enqueue(netMsgTrio);

            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
            int packed = writer.Pack(queue, 1024);
            Assert.That(packed, Is.EqualTo(3));

            var copyBuffer = new byte[1024];
            var size = writer.CopyBytes(copyBuffer);
            using var reader = MemoryManager.Instance.RecyclablePool.Get<ReadStream>();
            reader.Fill(copyBuffer, 0, size);
            var resultQueue = new Queue<NetMessage>();
            var unpacked = reader.Unpack(resultQueue);
            
            Assert.That(unpacked, Is.EqualTo(3));

            using var netMsgResult = resultQueue.Dequeue();
            var testTypeResult = (TestType) netMsgResult.Message;
            Assert.That(Math.Abs(testTypeResult.TestField.X), Is.EqualTo(Math.Abs(testType.TestField.X)).Within(tolerance));
            Assert.That(Math.Abs(testTypeResult.TestField.Y), Is.EqualTo(Math.Abs(testType.TestField.Y)).Within(tolerance));
            Assert.That(Math.Abs(testTypeResult.TestField.Z), Is.EqualTo(Math.Abs(testType.TestField.Z)).Within(tolerance));
            Assert.That(Math.Abs(testTypeResult.TestField.W), Is.EqualTo(Math.Abs(testType.TestField.W)).Within(tolerance));

            using var netMsgResultDuo = resultQueue.Dequeue();
            var testTypeResultDuo = (TestTypeDuo) netMsgResultDuo.Message;
            Assert.That(testTypeResultDuo.TestField.X, Is.EqualTo(testTypeDuo.TestField.X).Within(tolerance));
            Assert.That(testTypeResultDuo.TestField.Y, Is.EqualTo(testTypeDuo.TestField.Y).Within(tolerance));
            Assert.That(testTypeResultDuo.TestField.Z, Is.EqualTo(testTypeDuo.TestField.Z).Within(tolerance));

            tolerance = 0.0002f;
            using var netMsgResultTrio = resultQueue.Dequeue();
            var testTypeResultTrio = (TestTypeTrio) netMsgResultTrio.Message;
            Assert.That(testTypeResultTrio.TestField.X, Is.EqualTo(testTypeTrio.TestField.X).Within(tolerance));
            Assert.That(testTypeResultTrio.TestField.Y, Is.EqualTo(testTypeTrio.TestField.Y).Within(tolerance));
            Assert.That(testTypeResultTrio.TestField.Z, Is.EqualTo(testTypeTrio.TestField.Z).Within(tolerance));
        }
    }
}