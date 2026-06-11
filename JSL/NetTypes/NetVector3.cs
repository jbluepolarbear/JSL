// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.Buffers;
using JSL.Utility;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents a 3D vector serialized as three uncompressed 32-bit floats.
    /// </summary>
    public class NetVector3 : NetRecyclableSerializable
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="NetVector3"/> class.
        /// </summary>
        public NetVector3()
        {
            
        }
        
        /// <summary>
        /// Serializes the vector by writing X, Y, and Z as 32-bit floats to the write stream.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }

        /// <summary>
        /// Deserializes the vector by reading three 32-bit floats from the read stream.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
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
    }
}