// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.Buffers;
using JSL.NetTypes;

namespace JSL.Messages
{
    /// <summary>
    /// Represents the abstract base class for all serializable network messages.
    /// Manages common message metadata such as Message ID, source, destination, and frame number.
    /// </summary>
    public abstract partial class BaseMessage : NetRecyclableSerializable
    {
        /// <summary>
        /// Gets the unique identifier corresponding to the concrete message type.
        /// Used during serialization/deserialization to route messages to their correct handlers.
        /// </summary>
        public abstract uint TypeId { get; }

        /// <summary>
        /// The unique identifier for this specific message instance.
        /// </summary>
        public uint Id;

        /// <summary>
        /// The identifier of the sender (client or server) of this message.
        /// </summary>
        public uint FromId; // Client or server that sent message

        /// <summary>
        /// The identifier of the recipient of this message. A value of 0 means the message is broadcasted to all connections.
        /// </summary>
        public uint ToId; // 0 means every connection

        /// <summary>
        /// The simulation frame at which this message was generated or is meant to be processed.
        /// </summary>
        public uint Frame; // current game frame time is calculated from gamestart message time at fixed rate interval 

        /// <summary>
        /// Serializes the message metadata (Id, FromId, ToId, Frame) to the write stream.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            writer.Write((uint) Id);
            writer.Write((uint) FromId);
            writer.Write((uint) ToId);
            writer.Write((uint) Frame);
        }

        /// <summary>
        /// Deserializes the message metadata (Id, FromId, ToId, Frame) from the read stream.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            Id = reader.ReadUInt32();
            FromId = reader.ReadUInt32();
            ToId = reader.ReadUInt32();
            Frame = reader.ReadUInt32();
        }

        /// <summary>
        /// Custom implementation for acquiring the instance from the pool.
        /// </summary>
        protected override void AcquireImpl()
        {
            base.AcquireImpl();
        }

        /// <summary>
        /// Custom implementation for releasing the instance back to the pool.
        /// Resets message metadata fields.
        /// </summary>
        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Id = 0;
            FromId = 0;
            ToId = 0;
            Frame = 0;
        }

        /// <summary>
        /// Implicitly converts a <see cref="BaseMessage"/> to its string representation (the class name).
        /// </summary>
        /// <param name="netString">The message to convert.</param>
        public static implicit operator string(BaseMessage netString) => netString.GetType().ToString();
    }
}