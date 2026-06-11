// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.Buffers;
using JSL.Utility;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents a Quaternion optimized for network serialization, compressing it into 32 bits (2 bits for the largest component index, and 9 bits for each of the three other components).
    /// </summary>
    public class NetQuat : NetRecyclableSerializable
    {
        /// <summary>
        /// The minimum bound for compressed components (1/sqrt(2)).
        /// </summary>
        private const float Minimum = -1.0f / 1.414214f; // 1.0 / sqrt(2)

        /// <summary>
        /// The maximum bound for compressed components (1/sqrt(2)).
        /// </summary>
        private const float Maximum = +1.0f / 1.414214f;

        /// <summary>
        /// Number of bits used to represent each of the three smaller components.
        /// </summary>
        private const int Bits = 9;

        /// <summary>
        /// The scaling factor used to map normalized components to integers.
        /// </summary>
        private const float Scale = (float) ((1u << Bits) - 1u); // 2 ^ 9 - 1

        /// <summary>
        /// The inverse scaling factor used to reconstruct components during deserialization.
        /// </summary>
        private const float InverseScale = 1.0f / Scale;

        /// <summary>
        /// The index of the largest absolute component (0 = X, 1 = Y, 2 = Z, 3 = W).
        /// </summary>
        private uint _largest;

        /// <summary>
        /// The compressed integer representation of the first smaller component.
        /// </summary>
        private uint _integerA;

        /// <summary>
        /// The compressed integer representation of the second smaller component.
        /// </summary>
        private uint _integerB;

        /// <summary>
        /// The compressed integer representation of the third smaller component.
        /// </summary>
        private uint _integerC;

        /// <summary>
        /// Identifies the largest absolute component of the Quaternion and maps the other three components into their scaled integer representations.
        /// </summary>
        public void Load()
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

            _largest = largest;
            _integerA = (uint) (float) Math.Floor(normalA * Scale + 0.5f);
            _integerB = (uint) (float) Math.Floor(normalB * Scale + 0.5f);
            _integerC = (uint) (float) Math.Floor(normalC * Scale + 0.5f);
        }

        /// <summary>
        /// Reconstructs the full Quaternion (X, Y, Z, W) from the compressed largest component index and the three scaled integer components.
        /// </summary>
        public void Save()
        {
            var a = _integerA * InverseScale * (Maximum - Minimum) + Minimum;
            var b = _integerB * InverseScale * (Maximum - Minimum) + Minimum;
            var c = _integerC * InverseScale * (Maximum - Minimum) + Minimum;

            switch (_largest) {
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
        
        /// <summary>
        /// Compresses and serializes the Quaternion to the write stream using 32 bits.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            Load();
            writer.WriteBits(_largest, 2);
            writer.WriteBits(_integerA, Bits);
            writer.WriteBits(_integerB, Bits);
            writer.WriteBits(_integerC, Bits);
        }

        /// <summary>
        /// Deserializes and reconstructs the compressed Quaternion from the read stream.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            _largest = (uint) reader.ReadBits(2);
            _integerA = (uint) reader.ReadBits(Bits);
            _integerB = (uint) reader.ReadBits(Bits);
            _integerC = (uint) reader.ReadBits(Bits);
            Save();
        }

        /// <summary>
        /// The X component of the Quaternion.
        /// </summary>
        public float X;

        /// <summary>
        /// The Y component of the Quaternion.
        /// </summary>
        public float Y;

        /// <summary>
        /// The Z component of the Quaternion.
        /// </summary>
        public float Z;

        /// <summary>
        /// The W component of the Quaternion.
        /// </summary>
        public float W;
    }
}