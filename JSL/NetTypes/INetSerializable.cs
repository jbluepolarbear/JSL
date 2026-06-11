// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.Buffers;

namespace JSL.NetTypes
{
    /// <summary>
    /// Defines methods for serializing and deserializing network data using write and read streams.
    /// </summary>
    public interface INetSerializable
    {
        /// <summary>
        /// Serializes the object's data into the provided write stream.
        /// </summary>
        /// <param name="writer">The target write stream.</param>
        void Serialize(WriteStream writer);

        /// <summary>
        /// Deserializes the object's data from the provided read stream.
        /// </summary>
        /// <param name="reader">The source read stream.</param>
        void Deserialize(ReadStream reader);
    }
}