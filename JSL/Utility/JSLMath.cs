// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace JSL.Utility
{
    /// <summary>
    /// Collection of custom high-performance mathematical utility operations for JSL.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class JSLMath
    {
        /// <summary>
        /// Performs standard linear interpolation (Lerp) between two floats.
        /// </summary>
        /// <param name="a">The start value.</param>
        /// <param name="b">The end value.</param>
        /// <param name="t">The interpolation value, typically clamped between 0 and 1.</param>
        /// <returns>The interpolated float result.</returns>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        
        /// <summary>
        /// Calculates 2 raised to the power of the specified exponent using bit-shift operations.
        /// </summary>
        /// <param name="power">The exponent value.</param>
        /// <returns>2 ^ power.</returns>
        public static int PowerOf2(int power)
        {
            return 1 << power;
        }

        /// <summary>
        /// Constant multiplier to convert radians to degrees.
        /// </summary>
        public const float RadianToDegree = 57.2958f;

        /// <summary>
        /// Constant multiplier to convert degrees to radians.
        /// </summary>
        public const float DegreeToRadian = 1 / RadianToDegree;

        /// <summary>
        /// Converts a quaternion to euler angles (yaw, pitch, roll) in radians.
        /// </summary>
        /// <param name="x">Quaternion X component.</param>
        /// <param name="y">Quaternion Y component.</param>
        /// <param name="z">Quaternion Z component.</param>
        /// <param name="w">Quaternion W component.</param>
        /// <returns>A tuple representing Yaw (x), Pitch (y), and Roll (z) in radians.</returns>
        public static (float x, float y, float z) QuaternionToEuler(float x, float y, float z, float w)
        {
            var t0 = 2.0f * (w * x + y * z);
            var t1 = 1.0f - 2.0f * (x * x + y * y);
            var roll = (float) Math.Atan2(t0, t1);
            var t2 = 2.0f * (w * y - z * x);
            t2 = t2 > 1.0f ? 1.0f : t2;
            t2 = t2 < -1.0f ? -1.0f : t2;
            var pitch = (float) Math.Asin(t2);
            var t3 = 2.0f * (w * z + x * y);
            var t4 = 1.0f - 2.0f * (y * y + z * z);
            var yaw = (float) Math.Atan2(t3, t4);
            return (yaw, pitch, roll);
        }

        /// <summary>
        /// Converts euler angles (yaw, pitch, roll) in radians to a quaternion.
        /// </summary>
        /// <param name="yaw">Euler Yaw angle (rotation around Y axis).</param>
        /// <param name="pitch">Euler Pitch angle (rotation around X axis).</param>
        /// <param name="roll">Euler Roll angle (rotation around Z axis).</param>
        /// <returns>A tuple representing Quaternion components (X, Y, Z, W).</returns>
        public static (float x, float y, float z, float w) EulerToQuaternion(float yaw, float pitch, float roll)
        {
            var qx = (float) (Math.Sin(roll / 2.0f) * Math.Cos(pitch / 2.0f) * Math.Cos(yaw / 2.0f) - Math.Cos(roll / 2.0f) * Math.Sin(pitch / 2.0f) * Math.Sin(yaw / 2.0f));
            var qy = (float) (Math.Cos(roll / 2.0f) * Math.Sin(pitch / 2.0f) * Math.Cos(yaw / 2.0f) + Math.Sin(roll / 2.0f) * Math.Cos(pitch / 2.0f) * Math.Sin(yaw / 2.0f));
            var qz = (float) (Math.Cos(roll / 2.0f) * Math.Cos(pitch / 2.0f) * Math.Sin(yaw / 2.0f) - Math.Sin(roll / 2.0f) * Math.Sin(pitch / 2.0f) * Math.Cos(yaw / 2.0f));
            var qw = (float) (Math.Cos(roll / 2.0f) * Math.Cos(pitch / 2.0f) * Math.Cos(yaw / 2.0f) + Math.Sin(roll / 2.0f) * Math.Sin(pitch / 2.0f) * Math.Sin(yaw / 2.0f));
            return (qx, qy, qz, qw);
        }

        /// <summary>
        /// Converts a signed 32-bit integer to a ZigZag-encoded unsigned integer.
        /// </summary>
        /// <param name="value">The signed integer value.</param>
        /// <returns>The unsigned integer value.</returns>
        public static uint SignedToUnsigned(int value)
        {
            return (uint) ((value << 1) ^ (value >> 31));
        }

        /// <summary>
        /// Decodes a ZigZag-encoded unsigned 32-bit integer back to a signed integer.
        /// </summary>
        /// <param name="value">The unsigned integer value.</param>
        /// <returns>The decoded signed integer value.</returns>
        public static int UnsignedToSigned(uint value)
        {
            return (int) ((value >> 1) ^ -(value & 1));
        }
    }
}