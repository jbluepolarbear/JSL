using JSL.NetTypes;

namespace JSL.Pools
{
    /// <summary>
    /// keeps a cache of reusable net messages
    /// </summary>
    public class NetMessagePool
    {
        public NetMessage Get(uint typeId = 0)
        {
            var netMessage = MemoryManager.Instance.RecyclablePool.Get<NetMessage>();
            if (typeId != 0)
            {
                netMessage.Message = MemoryManager.Instance.GeneratedMessagePool.Get(typeId);
            }
            return netMessage;
        }
    }
}