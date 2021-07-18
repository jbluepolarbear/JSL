using System;
using JSL.Buffers;

namespace JSL.NetTypes
{
    public interface INetRecyclable: IDisposable
    {
        int RefCount { get; }
        IDisposable Acquire();
        public void Serialize(WriteStream writer);
        public void Deserialize(ReadStream reader);
    }
}