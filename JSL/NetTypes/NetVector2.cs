// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.Buffers;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents a 2D vector optimized for network serialization, compressing X and Y coordinates into 18 bits each.
    /// Range is clamped between -131072.0 and 131071.0.
    /// </summary>
    public class NetVector2 : NetRecyclableSerializable
    {
        /// <summary>
        /// The minimum bound for coordinates.
        /// </summary>
        private const float Minimum = -131072.0f;

        /// <summary>
        /// The maximum bound for coordinates.
        /// </summary>
        private const float Maximum = +131071.0f;

        /// <summary>
        /// The scaling factor used to map the normalized value to an 18-bit integer.
        /// </summary>
        private const float Scale = 262143.0f;

        /// <summary>
        /// The inverse scaling factor used to reconstruct coordinate values during deserialization.
        /// </summary>
        private const float InverseScale = 1.0f / Scale;
        
        /// <summary>
        /// Serializes the vector coordinates by mapping X and Y to 18-bit unsigned integers written to the stream.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            var normalA = (X - Minimum) / (Maximum - Minimum);
            var normalB = (Y - Minimum) / (Maximum - Minimum);

            var integerA = (uint) (normalA * Scale + 0.5f);
            var integerB = (uint) (normalB * Scale + 0.5f);
            writer.WriteBits(integerA, 18);
            writer.WriteBits(integerB, 18);
        }

        /// <summary>
        /// Deserializes the vector coordinates by reading two 18-bit unsigned integers and reconstructing X and Y.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            var integerA = reader.ReadBits(18);
            var integerB = reader.ReadBits(18);
            
            X = integerA * InverseScale * (Maximum - Minimum) + Minimum;
            Y = integerB * InverseScale * (Maximum - Minimum) + Minimum;
        }

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public float X;

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public float Y;
    }
}