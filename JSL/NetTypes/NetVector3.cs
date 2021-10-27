using System;
using JSL.Buffers;
using JSL.Utility;

namespace JSL.NetTypes
{
    public class NetVector3: NetRecyclableSerializable
    {
        public NetVector3()
        {
            
        }
        
        public override void Serialize(WriteStream writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }

        public override void Deserialize(ReadStream reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
        }

        public float X;
        public float Y;
        public float Z;
    }
}