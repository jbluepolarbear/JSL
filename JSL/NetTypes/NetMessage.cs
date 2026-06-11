// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using JSL.Buffers;
using JSL.Messages;

namespace JSL.NetTypes
{
    /// <summary>
    /// Allows generated messages to serialize and deserialize without reflection.
    /// Each generated type has a unique TypeId, and a corresponding <see cref="BaseMessage"/> is retrieved from the pool on deserialization.
    /// </summary>
    public class NetMessage : NetRecyclableSerializable
    {
        /// <summary>
        /// The underlying network message payload.
        /// </summary>
        public BaseMessage Message;

        /// <summary>
        /// Serializes the message payload to the write stream, writing its TypeId first.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        /// <exception cref="NullReferenceException">Thrown when <see cref="Message"/> is null.</exception>
        public override void Serialize(WriteStream writer)
        {
            if (Message == null)
            {
                throw new NullReferenceException("Message can't be null");
            }
            writer.Write(Message.TypeId);
            Message.Serialize(writer);
        }

        /// <summary>
        /// Deserializes the message payload from the read stream by reading the TypeId, acquiring the corresponding message instance from the pool, and deserializing its payload.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            var typeId = reader.ReadUInt32();
            Message = MemoryManager.GeneratedMessagePool.Get(typeId);
            Message.Deserialize(reader);
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
        /// Disposes and nullifies the nested <see cref="Message"/> instance.
        /// </summary>
        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Message?.Dispose();
            Message = null;
        }
    }
}