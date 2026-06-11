// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.Utility;
using JSL.Buffers;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace JSLTest
{
    [TestFixture]
    public class JSLMathTest
    {
        [Test]
        public void TestJSLMathAndMemoryHelpers()
        {
            // ZigZag Coding Roundtrip
            int[] testInts = { 0, -1, 1, 12345, -12345, int.MaxValue, int.MinValue };
            foreach (var val in testInts)
            {
                var unsigned = JSLMath.SignedToUnsigned(val);
                var signed = JSLMath.UnsignedToSigned(unsigned);
                Assert.AreEqual(val, signed);
            }

            // Memory Float/Int Roundtrip
            float[] testFloats = { 0.0f, -1.0f, 1.0f, 3.14159f, float.MaxValue, float.MinValue, float.NaN, float.PositiveInfinity };
            foreach (var val in testFloats)
            {
                var valInt = Memory.FloatToInt(val);
                var valFloat = Memory.IntToFloat(valInt);
                if (float.IsNaN(val))
                {
                    Assert.IsTrue(float.IsNaN(valFloat));
                }
                else
                {
                    Assert.AreEqual(val, valFloat);
                }
            }

            // Memory Double/Long Roundtrip
            double[] testDoubles = { 0.0, -1.0, 1.0, 3.1415926535, double.MaxValue, double.MinValue, double.NaN };
            foreach (var val in testDoubles)
            {
                var valLong = Memory.DoubleToLong(val);
                var valDouble = Memory.LongToDouble(valLong);
                if (double.IsNaN(val))
                {
                    Assert.IsTrue(double.IsNaN(valDouble));
                }
                else
                {
                    Assert.AreEqual(val, valDouble);
                }
            }

            // Euler / Quaternion conversions
            var (qx, qy, qz, qw) = JSLMath.EulerToQuaternion(0.5f, 0.2f, -0.1f);
            var (yaw, pitch, roll) = JSLMath.QuaternionToEuler(qx, qy, qz, qw);
            Assert.That(yaw, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(pitch, Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(roll, Is.EqualTo(-0.1f).Within(0.0001f));
        }
    }
}
