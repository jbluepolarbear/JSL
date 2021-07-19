using JSL.Utility;

namespace JSL.Pools
{
    public class MemoryManager: Singleton<MemoryManager>
    {
        public void Clear()
        {
            ListPool.Clear();
            RecyclablePool.Clear();
            GeneratedMessagePool.Clear();
        }
        
        public ListPool ListPool { get; } = new ListPool();
        public NetMessagePool MessagePool { get; } = new NetMessagePool();
        public NetRecyclablePool RecyclablePool { get; } = new NetRecyclablePool();
        public NetGeneratedMessagePool GeneratedMessagePool { get; } = new NetGeneratedMessagePool();
    }
}