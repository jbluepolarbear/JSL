using System.Collections.Generic;
using JSL.Buffers;
using JSL.NetTypes;
using JSL.Pools;
using JSL.Utility;

namespace JSL.Messages.Util
{
    public static class MessageHelpers
    {
        /// <summary>
        /// pack the writer with as many messages from the queue that fit.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="messages"></param>
        /// <param name="inMessages"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public static int Pack(this WriteStream writer, Queue<NetMessage> inMessages, int maxSize)
        {
            var startPosition = writer.GetBitsProcessed();
            var testSize = maxSize * 8;
            var packedMessages = 0;
            writer.Write(packedMessages);
            var lastOffset = writer.GetBitsProcessed();
            while (inMessages.Count > 0)
            {
                var netMessage = inMessages.Peek();
                writer.Write(netMessage);
                if (writer.GetBitsProcessed() > testSize)
                {
                    writer.SetBitPosition(lastOffset);
                    break;
                }
                inMessages.Dequeue();
                netMessage.Dispose();
                packedMessages++;
                lastOffset = writer.GetBitsProcessed();
            }

            writer.Flush();
            writer.SetBitPosition(startPosition);
            writer.Write(packedMessages);
            writer.SetBitPosition(lastOffset);

            Assert.True(packedMessages > 0);
            return packedMessages;
        }

        public static int Unpack(this ReadStream reader, Queue<NetMessage> outMessages)
        {
            var unpackedMessages = reader.ReadInt32();
            for (var i = 0; i < unpackedMessages; ++i)
            {
                var netMessage = MemoryManager.Instance.MessagePool.Get();
                netMessage.Deserialize(reader);
                outMessages.Enqueue(netMessage);
            }

            return unpackedMessages;
        }
    }
}