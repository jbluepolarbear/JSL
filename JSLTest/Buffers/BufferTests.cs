// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using JSL.Buffers;
using JSL.NetTypes;
using JSL.Pools;
using NUnit.Framework;

namespace JSLTest
{
    [TestFixture]
    public class BufferTests
    {
        [SetUp]
        public void Setup()
        {
            MemoryManager.Instance.Clear();
        }
        
        [Test]
        public void TestBitBuffers()
        {
            var bitWriter = new BitWriter(1024 * 4);
            bitWriter.WriteBits(127, 7);
            bitWriter.WriteBits('c', 16);
            var bytes = new byte[128];
            for (var i = 0; i < 128; ++i)
            {
                bytes[i] = (byte) i;
            }
            bitWriter.WriteAlign();
            bitWriter.WriteBytes(bytes, 128);
            bitWriter.FlushBits();
            var bitReader = new BitReader(bitWriter.AsReadOnlySpan());
            Assert.AreEqual(127, bitReader.ReadBits(7));
            Assert.AreEqual('c', bitReader.ReadBits(16));
            bitReader.ReadAlign();
            bitReader.ReadBytes(bytes);
            for (var i = 0; i < 128; ++i)
            {
                Assert.AreEqual((byte) i, bytes[i]);
            }
        }
        
        [Test]
        public void TestStreams()
        {
            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
            {
                var strBytes = Encoding.UTF8.GetBytes("hello there");
                writer.Write(strBytes.Length);
                writer.WriteBytes(strBytes, strBytes.Length);
            }
            writer.WriteBits(127, 7);
            writer.WriteBits('c', 16);
            writer.WriteIntRange(2048, 1024, 2048);
            writer.Write(123465643L);
            writer.Write(9223372036854775807UL);
            writer.Write(1.1234567f);
            writer.Write(1.123456789101112131415);
            using var netList = MemoryManager.Instance.RecyclablePool.Get<NetList<NetTransform>>();
            using var netTransform = MemoryManager.Instance.RecyclablePool.Get<NetTransform>();
            netTransform.Id.Active = true;
            netTransform.Id.Id = 15;
            netTransform.Position.X = 10.5f;
            netTransform.Position.Y = 15.0f;
            netTransform.Position.Z = 2.55f;
            netTransform.Rotation.X = 0.0f;
            netTransform.Rotation.Y = 0.0f;
            netTransform.Rotation.Z = 0.0f;
            netTransform.Rotation.W = 1.0f;
            netList.Add(netTransform);
            using var netTransform2 = MemoryManager.Instance.RecyclablePool.Get<NetTransform>();
            netTransform2.Id.Active = true;
            netTransform2.Id.Id = 15;
            netTransform2.Position.X = 10.5f;
            netTransform2.Position.Y = 15.0f;
            netTransform2.Position.Z = 2.55f;
            netTransform2.Rotation.X = 0.0f;
            netTransform2.Rotation.Y = 0.0f;
            netTransform2.Rotation.Z = 0.0f;
            netTransform2.Rotation.W = 1.0f;
            netList.Add(netTransform2);
            netList.Serialize(writer);
            var bytes = new byte[128];
            for (var i = 0; i < 128; ++i)
            {
                bytes[i] = (byte) i;
            }
            writer.WriteBytes(bytes, 128);


            var copyBuffer = new byte[1024];
            var size = writer.CopyBytes(copyBuffer);

            using var reader = MemoryManager.Instance.RecyclablePool.Get<ReadStream>();
            reader.Fill(copyBuffer, 0, size);
            {
                var length = reader.ReadInt32();
                var readBytes = new byte[length];
                reader.ReadBytes(readBytes);
                var readStr = Encoding.UTF8.GetString(readBytes);
                Assert.AreEqual(readStr, "hello there");
            }
            Assert.AreEqual(127, reader.ReadBits(7));
            Assert.AreEqual('c', reader.ReadBits(16));
            Assert.AreEqual(2048, reader.ReadIntRange(1024, 2048));
            Assert.AreEqual(123465643L, reader.ReadInt64());
            Assert.AreEqual(9223372036854775807UL, reader.ReadUInt64());
            Assert.AreEqual(1.1234567f, reader.ReadSingle());
            Assert.AreEqual(1.123456789101112131415, reader.ReadDouble());
            using var outNetList = MemoryManager.Instance.RecyclablePool.Get<NetList<NetTransform>>();
            outNetList.Deserialize(reader);
            const float tolerance = 0.02f;
            for (var i = 0; i < 2; ++i)
            {
                var transform1 = netList[i];
                var transform2 = outNetList[i];
                Assert.That(transform1.Id.Id, Is.EqualTo(transform2.Id.Id));
                Assert.That(transform1.Id.Active, Is.EqualTo(transform2.Id.Active));
                Assert.That(transform1.Position.X, Is.EqualTo(transform2.Position.X).Within(tolerance));
                Assert.That(transform1.Position.Y, Is.EqualTo(transform2.Position.Y).Within(tolerance));
                Assert.That(transform1.Position.Z, Is.EqualTo(transform2.Position.Z).Within(tolerance));
                Assert.That(transform1.Rotation.X, Is.EqualTo(transform2.Rotation.X).Within(tolerance));
                Assert.That(transform1.Rotation.Y, Is.EqualTo(transform2.Rotation.Y).Within(tolerance));
                Assert.That(transform1.Rotation.Z, Is.EqualTo(transform2.Rotation.Z).Within(tolerance));
                Assert.That(transform1.Rotation.W, Is.EqualTo(transform2.Rotation.W).Within(tolerance));
            }
            reader.ReadBytes(bytes);
            for (var i = 0; i < 128; ++i)
            {
                Assert.AreEqual((byte) i, bytes[i]);
            }
        }



        [Test]
        public void TestBufferOverflowEdgeCases()
        {
            // BitWriter overflow
            var bitWriter = new BitWriter(4); // 32 bits capacity
            bitWriter.WriteBits(15, 32);
            Assert.IsFalse(bitWriter.IsOverflow());
            bitWriter.WriteBits(1, 1);
            Assert.IsTrue(bitWriter.IsOverflow());

            // BitReader overflow
            var data = new uint[] { 12345 };
            var bitReader = new BitReader(data); // 32 bits capacity
            bitReader.ReadBits(32);
            Assert.IsFalse(bitReader.IsOverflow());
            bitReader.ReadBits(1);
            Assert.IsTrue(bitReader.IsOverflow());
        }

        [Test]
        public void TestStreamMagicCheck()
        {
            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
            writer.Check(0xABCDEF12);

            var buffer = new byte[128];
            var size = writer.CopyBytes(buffer);

            using var reader = MemoryManager.Instance.RecyclablePool.Get<ReadStream>();
            reader.Fill(buffer, 0, size);

            Assert.IsTrue(reader.Check(0xABCDEF12));

            reader.Reset();
            reader.Fill(buffer, 0, size);
            Assert.IsFalse(reader.Check(0x87654321));
        }

        [Test]
        public void TestUnalignedByteWriting()
        {
            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
            writer.WriteBits(7, 3); // Unaligned
            var payload = new byte[] { 10, 20, 30 };
            writer.WriteBytes(payload, 3); // WriteBytes aligns internally
            writer.WriteBits(3, 2);

            var buffer = new byte[128];
            var size = writer.CopyBytes(buffer);

            using var reader = MemoryManager.Instance.RecyclablePool.Get<ReadStream>();
            reader.Fill(buffer, 0, size);

            Assert.AreEqual(7, reader.ReadBits(3));
            var readPayload = new byte[3];
            reader.ReadBytes(readPayload); // ReadBytes aligns internally
            Assert.AreEqual(10, readPayload[0]);
            Assert.AreEqual(20, readPayload[1]);
            Assert.AreEqual(30, readPayload[2]);
            Assert.AreEqual(3, reader.ReadBits(2));
        }

        [Test]
        public void TestBitPackingVaryingWidths()
        {
            var bitWriter = new BitWriter(256);
            // Write varying width bit values
            for (var bits = 1; bits <= 32; ++bits)
            {
                var val = (uint)(0xFFFFFFFFu >> (32 - bits));
                bitWriter.WriteBits(val, bits);
            }
            bitWriter.FlushBits();

            var bitReader = new BitReader(bitWriter.AsReadOnlySpan());
            for (var bits = 1; bits <= 32; ++bits)
            {
                var expected = (uint)(0xFFFFFFFFu >> (32 - bits));
                var actual = bitReader.ReadBits(bits);
                Assert.AreEqual(expected, actual, $"Failed for bit width {bits}");
            }
        }

        [Test]
        public void TestWriteStreamAllTypes()
        {
            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
            writer.Write((byte)1); // bool as byte
            writer.Write((byte)0); // bool as byte
            writer.Write((byte)250);
            writer.Write((byte)136); // sbyte cast to byte
            writer.Write('Z');
            writer.Write((short)-32000);
            writer.Write((ushort)64000);
            writer.Write(-12345678);
            writer.Write(12345678u);
            writer.Write(-9876543210123L);
            writer.Write(9876543210123UL);
            writer.Write(123.456f);
            writer.Write(9876.543210123);

            var buffer = new byte[512];
            var size = writer.CopyBytes(buffer);

            using var reader = MemoryManager.Instance.RecyclablePool.Get<ReadStream>();
            reader.Fill(buffer, 0, size);

            Assert.AreEqual(1, reader.ReadByte());
            Assert.AreEqual(0, reader.ReadByte());
            Assert.AreEqual(250, reader.ReadByte());
            Assert.AreEqual(136, reader.ReadByte());
            Assert.AreEqual('Z', reader.ReadChar());
            Assert.AreEqual(-32000, reader.ReadInt16());
            Assert.AreEqual(64000, reader.ReadUInt16());
            Assert.AreEqual(-12345678, reader.ReadInt32());
            Assert.AreEqual(12345678u, reader.ReadUInt32());
            Assert.AreEqual(-9876543210123L, reader.ReadInt64());
            Assert.AreEqual(9876543210123UL, reader.ReadUInt64());
            Assert.AreEqual(123.456f, reader.ReadSingle());
            Assert.AreEqual(9876.543210123, reader.ReadDouble());
        }

        [Test]
        public void TestWriteIntRangeEdgeCases()
        {
            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
            // Range with negative min
            writer.WriteIntRange(-50, -100, 100);
            writer.WriteIntRange(100, -100, 100);
            writer.WriteIntRange(-100, -100, 100);
            // Large range
            writer.WriteIntRange(100000, 0, 100000);
            writer.WriteIntRange(0, 0, 100000);

            var buffer = new byte[256];
            var size = writer.CopyBytes(buffer);

            using var reader = MemoryManager.Instance.RecyclablePool.Get<ReadStream>();
            reader.Fill(buffer, 0, size);

            Assert.AreEqual(-50, reader.ReadIntRange(-100, 100));
            Assert.AreEqual(100, reader.ReadIntRange(-100, 100));
            Assert.AreEqual(-100, reader.ReadIntRange(-100, 100));
            Assert.AreEqual(100000, reader.ReadIntRange(0, 100000));
            Assert.AreEqual(0, reader.ReadIntRange(0, 100000));
        }

        [Test]
        public void TestResetAndReuse()
        {
            var bitWriter = new BitWriter(16);
            bitWriter.WriteBits(42, 6);
            bitWriter.Reset();
            bitWriter.WriteBits(99, 8);
            bitWriter.FlushBits();

            var bitReader = new BitReader(bitWriter.AsReadOnlySpan());
            Assert.AreEqual(99, bitReader.ReadBits(8));

            bitReader.Reset();
            Assert.AreEqual(99, bitReader.ReadBits(8));
        }
    }
}