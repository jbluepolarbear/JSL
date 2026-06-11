// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JSL.Buffers;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents network player information, comprising a network ID and a player name string.
    /// </summary>
    public class NetPlayerInfo : NetRecyclableSerializable
    {
        /// <summary>
        /// The network identifier of the player.
        /// </summary>
        public NetId Id;

        /// <summary>
        /// The name of the player.
        /// </summary>
        public NetString Name;

        /// <summary>
        /// Serializes the player information (ID and name) to the write stream.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            Id.Serialize(writer);
            Name.Serialize(writer);
        }

        /// <summary>
        /// Deserializes the player information (ID and name) from the read stream.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            Id.Deserialize(reader);
            Name.Deserialize(reader);
        }

        /// <summary>
        /// Custom implementation for acquiring the recyclable instance from the pool.
        /// Allocates new ID and Name instances from their respective pools.
        /// </summary>
        protected override void AcquireImpl()
        {
            base.AcquireImpl();
            Id = MemoryManager.RecyclablePool.Get<NetId>();
            Name = MemoryManager.RecyclablePool.Get<NetString>();
        }

        /// <summary>
        /// Custom implementation for releasing the recyclable instance back to the pool.
        /// Disposes and nullifies ID and Name instances.
        /// </summary>
        protected override void ReleaseImpl()
        {
            base.ReleaseImpl();
            Id.Dispose();
            Id = null;
            Name.Dispose();
            Name = null;
        }
    }
}