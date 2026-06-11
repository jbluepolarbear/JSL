// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.Buffers;
using JSL.Utility;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents a 3D vector compressed for network serialization (X: 18 bits, Y: 18 bits, Z: 14 bits).
    /// Range for X, Y, and Z is clamped between -256.0 and 255.0.
    /// </summary>
    public class NetCompressedVector3 : NetRecyclableSerializable
    {
        /// <summary>
        /// The minimum bound for the X coordinate.
        /// </summary>
        private const float MinimumX = -256.0f;

        /// <summary>
        /// The maximum bound for the X coordinate.
        /// </summary>
        private const float MaximumX = 255.0f;

        /// <summary>
        /// The minimum bound for the Y coordinate.
        /// </summary>
        private const float MinimumY = -256.0f;

        /// <summary>
        /// The maximum bound for the Y coordinate.
        /// </summary>
        private const float MaximumY = 255.0f;

        /// <summary>
        /// The minimum bound for the Z coordinate.
        /// </summary>
        private const float MinimumZ = -256.0f;

        /// <summary>
        /// The maximum bound for the Z coordinate.
        /// </summary>
        private const float MaximumZ = 255.0f;

        /// <summary>
        /// Number of bits used to serialize the X coordinate.
        /// </summary>
        private const int BitsX = 18;

        /// <summary>
        /// Number of bits used to serialize the Y coordinate.
        /// </summary>
        private const int BitsY = 18;

        /// <summary>
        /// Number of bits used to serialize the Z coordinate.
        /// </summary>
        private const int BitsZ = 14;

        /// <summary>
        /// Scaling factor for mapping normalized X to its integer range.
        /// </summary>
        private const float ScaleX = (float) ((1u << BitsX) - 1u);

        /// <summary>
        /// Scaling factor for mapping normalized Y to its integer range.
        /// </summary>
        private const float ScaleY = (float) ((1u << BitsY) - 1u);

        /// <summary>
        /// Scaling factor for mapping normalized Z to its integer range.
        /// </summary>
        private const float ScaleZ = (float) ((1u << BitsZ) - 1u);

        /// <summary>
        /// Inverse scaling factor to reconstruct the X coordinate.
        /// </summary>
        private const float InverseScaleX = 1.0f / ScaleX;

        /// <summary>
        /// Inverse scaling factor to reconstruct the Y coordinate.
        /// </summary>
        private const float InverseScaleY = 1.0f / ScaleY;

        /// <summary>
        /// Inverse scaling factor to reconstruct the Z coordinate.
        /// </summary>
        private const float InverseScaleZ = 1.0f / ScaleZ;

        /// <summary>
        /// Instantiates a new instance of the <see cref="NetCompressedVector3"/> class.
        /// </summary>
        public NetCompressedVector3()
        {
            
        }

        /// <summary>
        /// Compresses the current X, Y, and Z floating-point coordinates into their corresponding integer representation.
        /// </summary>
        public void Load()
        {
            var normalA = (X - MinimumX) / (MaximumX - MinimumX);
            var normalB = (Y - MinimumY) / (MaximumY - MinimumY);
            var normalC = (Z - MinimumZ) / (MaximumZ - MinimumZ);

            _integerA = (uint) (float) Math.Floor(normalA * ScaleX + 0.5f);
            _integerB = (uint) (float) Math.Floor(normalB * ScaleY + 0.5f);
            _integerC = (uint) (float) Math.Floor(normalC * ScaleZ + 0.5f);
        }

        /// <summary>
        /// Reconstructs the X, Y, and Z floating-point coordinates from their compressed integer representation.
        /// </summary>
        public void Save()
        {
            X = _integerA * InverseScaleX * (MaximumX - MinimumX) + MinimumX;
            Y = _integerB * InverseScaleY * (MaximumY - MinimumY) + MinimumY;
            Z = _integerC * InverseScaleZ * (MaximumZ - MinimumZ) + MinimumZ;
        }
        
        /// <summary>
        /// Compresses and serializes the 3D vector coordinates to the write stream.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            Load();
            writer.WriteBits(_integerA, BitsX);
            writer.WriteBits(_integerB, BitsY);
            writer.WriteBits(_integerC, BitsZ);
        }

        /// <summary>
        /// Deserializes and reconstructs the compressed 3D vector coordinates from the read stream.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            _integerA = reader.ReadBits(BitsX);
            _integerB = reader.ReadBits(BitsY);
            _integerC = reader.ReadBits(BitsZ);
            Save();
        }

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public float X;

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public float Y;

        /// <summary>
        /// The Z coordinate.
        /// </summary>
        public float Z;
        
        /// <summary>
        /// Compressed integer representation of the X coordinate.
        /// </summary>
        private uint _integerA;

        /// <summary>
        /// Compressed integer representation of the Y coordinate.
        /// </summary>
        private uint _integerB;

        /// <summary>
        /// Compressed integer representation of the Z coordinate.
        /// </summary>
        private uint _integerC;
    }
}