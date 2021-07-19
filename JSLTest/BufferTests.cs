using JSL.Buffers;
using JSL.NetTypes;
using JSL.Pools;
using NUnit.Framework;

namespace JSLTest
{
    [NonParallelizable]
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
            var bitWriter = new BitWriter(new uint[1024], 1024 * 4);
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
            var bitReader = new BitReader(bitWriter.GetData(), 1024);
            Assert.AreEqual(127, bitReader.ReadBits(7));
            Assert.AreEqual('c', bitReader.ReadBits(16));
            bitReader.ReadAlign();
            bitReader.ReadBytes(bytes, 128);
            for (var i = 0; i < 128; ++i)
            {
                Assert.AreEqual((byte) i, bytes[i]);
            }
        }
        
        [Test]
        public void TestStreams()
        {
            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
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
            writer.Write(netList);
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
            Assert.AreEqual(127, reader.ReadBits(7));
            Assert.AreEqual('c', reader.ReadBits(16));
            Assert.AreEqual(2048, reader.ReadIntRange(1024, 2048));
            Assert.AreEqual(123465643L, reader.ReadInt64());
            Assert.AreEqual(9223372036854775807UL, reader.ReadUInt64());
            Assert.AreEqual(1.1234567f, reader.ReadSingle());
            Assert.AreEqual(1.123456789101112131415, reader.ReadDouble());
            using var outNetList = MemoryManager.Instance.RecyclablePool.Get<NetList<NetTransform>>();
            reader.Read(outNetList);
            reader.ReadBytes(bytes, 128);
            for (var i = 0; i < 128; ++i)
            {
                Assert.AreEqual((byte) i, bytes[i]);
            }
        }
    }
}