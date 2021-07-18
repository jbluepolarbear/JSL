using System.Collections.Generic;
using JSL.Messages;

namespace JSL.Pools
{
    public class NetGeneratedMessagePool
    {
        public delegate BaseMessage Factory(uint classId);

        private Factory _messageFactory;
        public void RegisterMessageFactory(Factory factory)
        {
            _messageFactory = factory;
        }
        
        private Dictionary<uint, NetBasePool<BaseMessage>> _messagePools = new Dictionary<uint, NetBasePool<BaseMessage>>();

        public BaseMessage Get(uint typeId)
        {
            if (!TryGet(typeId, out var messagePool))
            {
                GetMessagePool(typeId, out messagePool);
            }
            var message = messagePool.Get();
            return message;
        }

        private void GetMessagePool(uint typeId, out NetBasePool<BaseMessage> messagePool)
        {
            messagePool = new NetBasePool<BaseMessage>(() => MessageFromTypeId(typeId));
            _messagePools.Add(typeId, messagePool);
        }

        private bool TryGet(uint typeId, out NetBasePool<BaseMessage> messagePool)
        {
            foreach (var pool in _messagePools)
            {
                if (pool.Key == typeId)
                {
                    messagePool = pool.Value;
                    return true;
                }
            }

            messagePool = null;
            return false;
        }

        private BaseMessage MessageFromTypeId(uint typeId)
        {
            return _messageFactory(typeId);
        }
    }
}