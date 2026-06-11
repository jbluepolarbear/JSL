// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.Buffers;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents a serialized network wrapper for enum values, storing the value as a byte.
    /// </summary>
    public class NetEnum : NetRecyclableSerializable
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="NetEnum"/> class.
        /// </summary>
        public NetEnum()
        {
            
        }
        
        /// <summary>
        /// The underlying byte representation of the enum value.
        /// </summary>
        public byte _value;
        
        /// <summary>
        /// Serializes the enum value as a byte to the write stream.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            writer.Write(_value);
        }

        /// <summary>
        /// Deserializes the enum value as a byte from the read stream.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            _value = reader.ReadByte();
        }
    }

    /// <summary>
    /// Represents a strongly-typed generic network wrapper for an enum of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The enum type, which must be a struct and an Enum.</typeparam>
    public class NetEnum<T> : NetEnum where T : struct, Enum
    {
        /// <summary>
        /// Gets or sets the strongly-typed enum value.
        /// </summary>
        public T Value
        {
            get => System.Runtime.CompilerServices.Unsafe.As<byte, T>(ref _value);
            set => _value = System.Runtime.CompilerServices.Unsafe.As<T, byte>(ref value);
        }
    }
}