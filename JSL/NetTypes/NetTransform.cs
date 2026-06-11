// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.Buffers;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents a network transform composed of a network ID, position vector, and rotation quaternion.
    /// Position and rotation are serialized only if the network identifier is active.
    /// </summary>
    public class NetTransform : NetRecyclableSerializable
    {
        /// <summary>
        /// Serializes the transform to the write stream.
        /// Serializes the ID, and conditionally serializes position and rotation if the ID is active.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            Id.Serialize(writer);
            if (Id.Active)
            {
                Position.Serialize(writer);
                Rotation.Serialize(writer);
            }
        }

        /// <summary>
        /// Deserializes the transform from the read stream.
        /// Deserializes the ID, and conditionally deserializes position and rotation if the ID is active.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            Id.Deserialize(reader);
            if (Id.Active)
            {
                Position.Deserialize(reader);
                Rotation.Deserialize(reader);
            }
        }

        /// <summary>
        /// Custom implementation for acquiring the recyclable instance from the pool.
        /// Allocates new ID, Position, and Rotation instances from their respective pools.
        /// </summary>
        protected override void AcquireImpl()
        {
            base.AcquireImpl();
            Id = MemoryManager.RecyclablePool.Get<NetId>();
            Position = MemoryManager.RecyclablePool.Get<NetVector3>();
            Rotation = MemoryManager.RecyclablePool.Get<NetQuat>();
        }

        /// <summary>
        /// Custom implementation for releasing the recyclable instance back to the pool.
        /// Disposes and nullifies ID, Position, and Rotation instances.
        /// </summary>
        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Id.Dispose();
            Id = null;
            Position.Dispose();
            Position = null;
            Rotation.Dispose();
            Rotation = null;
        }

        /// <summary>
        /// The network identifier (16 bits serialized).
        /// </summary>
        public NetId Id; // 16 bits

        /// <summary>
        /// The position vector.
        /// </summary>
        public NetVector3 Position; // 56 bits

        /// <summary>
        /// The rotation quaternion.
        /// </summary>
        public NetQuat Rotation; // 32 bits

        /// <summary>
        /// The maximum number of entities that can fit in a single data frame message.
        /// </summary>
        public const int MaxEntities = 78;
    }
}