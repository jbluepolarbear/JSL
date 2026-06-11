// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.Buffers;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents a floating-point value compressed and clamped within the range of [-1.0, 1.0] inclusive.
    /// Uses 16-bit precision serialization.
    /// </summary>
    public class NetUnitFloat : NetRecyclableSerializable
    {
        /// <summary>
        /// The minimum bound of the float range.
        /// </summary>
        private const float Minimum = -1.0f;

        /// <summary>
        /// The maximum bound of the float range.
        /// </summary>
        private const float Maximum = 1.0f;

        /// <summary>
        /// The scaling factor used to map the normalized value to a 16-bit integer.
        /// </summary>
        private const float Scale = 65535.0f; // 2 ^ 16 - 1

        /// <summary>
        /// The inverse scaling factor used to reconstruct the float value during deserialization.
        /// </summary>
        private const float InverseScale = 1.0f / Scale;
        
        /// <summary>
        /// Serializes the clamped float value by normalizing and mapping it to a 16-bit unsigned integer written to the stream.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            var normal = (_value - Minimum) / (Maximum - Minimum);
            var integer = (ushort) (normal * Scale + 0.5f);
            writer.Write(integer);
        }

        /// <summary>
        /// Deserializes the float value by reading a 16-bit unsigned integer and mapping it back to the [-1.0, 1.0] range.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            var integer = reader.ReadUInt16();
            _value = integer * InverseScale * (Maximum - Minimum) + Minimum;
        }

        /// <summary>
        /// The underlying float value.
        /// </summary>
        private float _value;

        /// <summary>
        /// Gets or sets the float value, clamped between -1.0f and 1.0f.
        /// </summary>
        public float Value
        {
            get => _value;
            set => _value = Math.Max(Math.Min(1.0f, value), -1.0f);
        }
    }
}