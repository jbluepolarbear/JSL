using System;
using JSL.Buffers;
using JSL.NetTypes;
using JSL.Pools;
using JSL.Utility;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace JSLTest
{
    [TestFixture]
    public class NetVec3Test
    {
        
        [SetUp]
        public void Setup()
        {
            MemoryManager.Instance.Clear();
        }

        [Test]
        public void InstantiateNetVector3()
        {
            float startX = 0.0f;
            float startY = 0.0f;
            float startZ = 0.0f;
            float endX = 10.0f;
            float endY = 10.0f;
            float endZ = 10.0f;
            var tolerance = 0.002f;
            var step = 1.0f / 20.0f;
            for (var i = 0; i < 20; ++i)
            {
                var t = i * step;
                var x = JSLMath.Lerp(startX, endX, t);
                var y = JSLMath.Lerp(startY, endY, t);
                var z = JSLMath.Lerp(startZ, endZ, t);
                using var netVector3 = MemoryManager.Instance.RecyclablePool.Get<NetVector3>();
                netVector3.X = x;
                netVector3.Y = y;
                netVector3.Z = z;
                netVector3.Load();
                netVector3.Save();
                Assert.That(Math.Abs(x), Is.EqualTo(Math.Abs(netVector3.X)).Within(tolerance));
                Assert.That(Math.Abs(y), Is.EqualTo(Math.Abs(netVector3.Y)).Within(tolerance));
                Assert.That(Math.Abs(z), Is.EqualTo(Math.Abs(netVector3.Z)).Within(tolerance));
            }
        }

        [Test]
        public void SerializeNetVector3()
        {
            float startX = 0.0f;
            float startY = 0.0f;
            float startZ = 0.0f;
            float endX = 10.0f;
            float endY = 10.0f;
            float endZ = 10.0f;
            var tolerance = 0.002f;
            var step = 1.0f / 20.0f;
            using var netList = MemoryManager.Instance.RecyclablePool.Get<NetList<NetVector3>>();
            for (var i = 0; i < 20; ++i)
            {
                var t = i * step;
                var x = JSLMath.Lerp(startX, endX, t);
                var y = JSLMath.Lerp(startY, endY, t);
                var z = JSLMath.Lerp(startZ, endZ, t);
                using var netVector3 = MemoryManager.Instance.RecyclablePool.Get<NetVector3>();
                netVector3.X = x;
                netVector3.Y = y;
                netVector3.Z = z;
                netList.Add(netVector3);
            }
            using var writer = MemoryManager.Instance.RecyclablePool.Get<WriteStream>();
            netList.Serialize(writer);
            
            var copyBuffer = new byte[1024];
            var size = writer.CopyBytes(copyBuffer);

            using var reader = MemoryManager.Instance.RecyclablePool.Get<ReadStream>();
            reader.Fill(copyBuffer, 0, size);

            using var outNetList = MemoryManager.Instance.RecyclablePool.Get<NetList<NetVector3>>();
            outNetList.Deserialize(reader);
            for (var i = 0; i < 20; ++i)
            {
                Assert.That(Math.Abs(netList[i].X), Is.EqualTo(Math.Abs(outNetList[i].X)).Within(tolerance));
                Assert.That(Math.Abs(netList[i].Y), Is.EqualTo(Math.Abs(outNetList[i].Y)).Within(tolerance));
                Assert.That(Math.Abs(netList[i].Z), Is.EqualTo(Math.Abs(outNetList[i].Z)).Within(tolerance));
            }
        }
    }
}