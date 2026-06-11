// Copyright (c) 2026 Jeremy Anderson (github: jbluepolarbear, email: jbluepolarbear@gmail.com, website: jeremyrobertanderson.com)
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using JSL.Buffers;

namespace JSL.NetTypes
{
    /// <summary>
    /// Represents a serialized network wrapper for string values.
    /// </summary>
    public class NetString : NetRecyclableSerializable
    {
        /// <summary>
        /// Instantiates a new empty instance of the <see cref="NetString"/> class.
        /// </summary>
        public NetString()
        {
        }
        
        /// <summary>
        /// Instantiates a new instance of the <see cref="NetString"/> class with an initial string value.
        /// </summary>
        /// <param name="inString">The initial string value.</param>
        public NetString(string inString)
        {
            String = inString;
        }
        
        /// <summary>
        /// The underlying string value.
        /// </summary>
        public string String;

        /// <summary>
        /// Serializes the string by writing its length and then each character sequentially to the write stream.
        /// </summary>
        /// <param name="writer">The stream to write serialization data to.</param>
        public override void Serialize(WriteStream writer)
        {
            writer.Write(String.Length);
            foreach (var c in String)
            {
                writer.Write(c);
            }
        }

        /// <summary>
        /// Thread-static string builder used to avoid allocations during deserialization.
        /// </summary>
        [ThreadStatic]
        private static StringBuilder _stringBuilder;

        /// <summary>
        /// Gets and clears a thread-static <see cref="StringBuilder"/> instance.
        /// </summary>
        /// <returns>A cleared <see cref="StringBuilder"/> instance ready for reuse.</returns>
        private static StringBuilder GetStringBuilder()
        {
            if (_stringBuilder == null)
            {
                _stringBuilder = new StringBuilder(2048);
            }
            _stringBuilder.Clear();
            return _stringBuilder;
        }

        /// <summary>
        /// Deserializes the string by reading its length and then reconstructing the characters from the read stream.
        /// </summary>
        /// <param name="reader">The stream to read serialization data from.</param>
        public override void Deserialize(ReadStream reader)
        {
            var sb = GetStringBuilder();
            var length = reader.ReadInt32();
            for (var i = 0; i < length; ++i)
            {
                sb.Append(reader.ReadChar());
            }

            String = sb.ToString();
        }
    }
}