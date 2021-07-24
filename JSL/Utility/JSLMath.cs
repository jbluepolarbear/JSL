using System;
using System.Collections.Generic;

namespace JSL.Utility
{
    public static class JSLMath
    {
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        
        public static int PowerOf2(int power)
        {
            if (_powerOf2Cache.Count <= power)
            {
                var value = PowerOf2(power - 1) * 2;
                _powerOf2Cache.Add(value);
            }

            return _powerOf2Cache[power];
        }

        public const float RadianToDegree = 57.2958f;
        public const float DegreeToRadian = 1 / RadianToDegree;

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

        public static (float x, float y, float z, float w) EulerToQuaternion(float yaw, float pitch, float roll)
        {
            var qx = (float) (Math.Sin(roll / 2.0f) * Math.Cos(pitch / 2.0f) * Math.Cos(yaw / 2.0f) - Math.Cos(roll / 2.0f) * Math.Sin(pitch / 2.0f) * Math.Sin(yaw / 2.0f));
            var qy = (float) (Math.Cos(roll / 2.0f) * Math.Sin(pitch / 2.0f) * Math.Cos(yaw / 2.0f) + Math.Sin(roll / 2.0f) * Math.Cos(pitch / 2.0f) * Math.Sin(yaw / 2.0f));
            var qz = (float) (Math.Cos(roll / 2.0f) * Math.Cos(pitch / 2.0f) * Math.Sin(yaw / 2.0f) - Math.Sin(roll / 2.0f) * Math.Sin(pitch / 2.0f) * Math.Cos(yaw / 2.0f));
            var qw = (float) (Math.Cos(roll / 2.0f) * Math.Cos(pitch / 2.0f) * Math.Cos(yaw / 2.0f) + Math.Sin(roll / 2.0f) * Math.Sin(pitch / 2.0f) * Math.Sin(yaw / 2.0f));
            return (qx, qy, qz, qw);
        }
        
        private static List<int> _powerOf2Cache = new List<int>(64)
        {
            1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536
        };
    }
}