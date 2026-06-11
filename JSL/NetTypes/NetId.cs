// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.Buffers;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents a network identifier capable of serialization, supporting IDs up to 2^15 and encoding active state.
    /// </summary>
    public class NetId : NetRecyclableSerializable
    {
        /// <summary>
        /// Serializes the network identifier to the provided write stream.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            writer.WriteBits(Id, 15);
            writer.WriteBits(Active ? 1u : 0u, 1);
        }

        /// <summary>
        /// Deserializes the network identifier from the provided read stream.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            Id = (ushort) reader.ReadBits(15);
            Active = reader.ReadBits(1) == 1;
        }

        /// <summary>
        /// The unique ID value (up to 32767).
        /// </summary>
        public ushort Id; // only Ids up to 2^15. first bit is for active state

        /// <summary>
        /// Indicates whether the network object or identifier is active.
        /// </summary>
        public bool Active;
        
        /// <summary>
        /// Implicitly converts a <see cref="NetId"/> to a <see cref="ushort"/>.
        /// </summary>
        /// <param name="netId">The network identifier to convert.</param>
        public static implicit operator ushort(NetId netId) => netId.Id;

        /// <summary>
        /// Implicitly converts a <see cref="ushort"/> ID to a new active <see cref="NetId"/>.
        /// </summary>
        /// <param name="id">The ID value to convert.</param>
        public static implicit operator NetId(ushort id) => new NetId { Id = id, Active = true };
    }
}