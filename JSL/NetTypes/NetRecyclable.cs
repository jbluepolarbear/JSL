using System;
using JSL.Buffers;
using JSL.Pools;

namespace JSL.NetTypes
{
    public abstract class NetRecyclable: INetRecyclable
    {
        public int RefCount { get; private set; } = 0;
        public bool InstanceActive { get; private set; } = false;
        public IDisposable Acquire()
        {
            if (RefCount++ == 0)
            {
                AcquireImpl();
            }

            return this;
        }

        private void Release()
        {
            if (RefCount == 0)
            {
                return;
            }

            if (--RefCount > 0)
            {
                return;
            }

            ReleaseImpl();
        }

        /// <summary>
        /// Called when Recycle Members should be acquired (call Get<T>())
        /// </summary>
        protected virtual void AcquireImpl()
        {
            InstanceActive = true;
        }

        protected virtual void ReleaseImpl()
        {
            InstanceActive = false;
        }

        public abstract void Serialize(WriteStream writer);

        public abstract void Deserialize(ReadStream reader);

        protected MemoryManager MemoryManager => MemoryManager.Instance;

        public void Dispose()
        {
            Release();
        }
    }
}