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
    public class NetQuatTest
    {
        
        [SetUp]
        public void Setup()
        {
            MemoryManager.Instance.Clear();
        }

        [Test]
        public void InstantiateNetQuat()
        {
            var yaw = 0.0f;
            var pitch = 0.0f;
            var roll = 0.0f;
            var step = 360.0f * JSLMath.DegreeToRadian / 20.0f;
            var tolerance = 0.002f;
            for (var i = 0; i < 20; ++i)
            {
                yaw -= step;
                pitch += step;
                roll += step;
                using var netQuat = MemoryManager.Instance.RecyclablePool.Get<NetQuat>();
                var quat = JSLMath.EulerToQuaternion(yaw, pitch, roll);
                netQuat.X = quat.x;
                netQuat.Y = quat.y;
                netQuat.Z = quat.z;
                netQuat.W = quat.w;
                netQuat.Load();
                netQuat.Save();
                Assert.That(Math.Abs(quat.x), Is.EqualTo(Math.Abs(netQuat.X)).Within(tolerance));
                Assert.That(Math.Abs(quat.y), Is.EqualTo(Math.Abs(netQuat.Y)).Within(tolerance));
                Assert.That(Math.Abs(quat.z), Is.EqualTo(Math.Abs(netQuat.Z)).Within(tolerance));
                Assert.That(Math.Abs(quat.w), Is.EqualTo(Math.Abs(netQuat.W)).Within(tolerance));
            }
        }
    }
}