using System;
using JSL.Buffers;
using JSL.Utility;
using Math = System.Math;

namespace JSL.NetTypes
{
    // Compresses Quaternion into 32 bits or an unsigned int
    public class NetQuat: NetRecyclable
    {
        
        private const float Minimum = -1.0f / 1.414214f; // 1.0 / sqrt(2)
        private const float Maximum = +1.0f / 1.414214f;

        private const int Bits = 9;
        private const float Scale = (float) ((1 << Bits) - 1); // 2 ^ 9 - 1
        private const float InverseScale = 1.0f / Scale;
        
        public override void Serialize(WriteStream writer)
        {
            var absX = Math.Abs(X);
            var absY = Math.Abs(Y);
            var absZ = Math.Abs(Z);
            var absW = Math.Abs(W);

            var largest = 0u;
            var largestValue = absX;

            if (absY > largestValue) {
                largest = 1;
                largestValue = absY;
            }

            if (absZ > largestValue) {
                largest = 2;
                largestValue = absZ;
            }

            if (absW > largestValue) {
                largest = 3;
            }

            var a = 0.0f;
            var b = 0.0f;
            var c = 0.0f;

            switch (largest) {
                case 0:
                    if (X >= 0) {
                        a = Y;
                        b = Z;
                        c = W;
                    } else {
                        a = -Y;
                        b = -Z;
                        c = -W;
                    }
                    break;

                case 1:
                    if (Y >= 0) {
                        a = X;
                        b = Z;
                        c = W;
                    } else {
                        a = -X;
                        b = -Z;
                        c = -W;
                    }
                    break;

                case 2:
                    if (Z >= 0) {
                        a = X;
                        b = Y;
                        c = W;
                    } else {
                        a = -X;
                        b = -Y;
                        c = -W;
                    }
                    break;

                case 3:
                    if (W >= 0) {
                        a = X;
                        b = Y;
                        c = Z;
                    } else {
                        a = -X;
                        b = -Y;
                        c = -Z;
                    }
                    break;
            }

            var normalA = (a - Minimum) / (Maximum - Minimum);
            var normalB = (b - Minimum) / (Maximum - Minimum);
            var normalC = (c - Minimum) / (Maximum - Minimum);

            var integerA = (uint) Math.Floor(normalA * Scale + 0.5f);
            var integerB = (uint) Math.Floor(normalB * Scale + 0.5f);
            var integerC = (uint) Math.Floor(normalC * Scale + 0.5f);

            writer.WriteBits(largest, 2);
            writer.WriteBits(integerA, Bits);
            writer.WriteBits(integerB, Bits);
            writer.WriteBits(integerC, Bits);
        }

        public override void Deserialize(ReadStream reader)
        {
            var largest = (uint) reader.ReadBits(2);
            var integerA = (uint) reader.ReadBits(Bits);
            var integerB = (uint) reader.ReadBits(Bits);
            var integerC = (uint) reader.ReadBits(Bits);
            
            var a = integerA * InverseScale * (Maximum - Minimum) + Minimum;
            var b = integerB * InverseScale * (Maximum - Minimum) + Minimum;
            var c = integerC * InverseScale * (Maximum - Minimum) + Minimum;

            switch (largest) {
                case 0:
                {
                    X = (float) Math.Sqrt(1 - a * a - b * b - c * c);
                    Y = a;
                    Z = b;
                    W = c;
                }
                    break;

                case 1:
                {
                    X = a;
                    Y = (float) Math.Sqrt(1 - a * a - b * b - c * c);
                    Z = b;
                    W = c;
                }
                    break;

                case 2:
                {
                    X = a;
                    Y = b;
                    Z = (float) Math.Sqrt(1 - a * a - b * b - c * c);
                    W = c;
                }
                    break;

                case 3:
                {
                    X = a;
                    Y = b;
                    Z = c;
                    W = (float) Math.Sqrt(1 - a * a - b * b - c * c);
                }
                    break;
            }
            
            Assert.True(!float.IsNaN(X));
            Assert.True(!float.IsNaN(Y));
            Assert.True(!float.IsNaN(Z));
            Assert.True(!float.IsNaN(W));
        }

        public float X;
        public float Y;
        public float Z;
        public float W;
    }
}