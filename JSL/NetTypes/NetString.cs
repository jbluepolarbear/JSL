using System.Text;
using JSL.Buffers;

namespace JSL.NetTypes
{
    public class NetString: NetRecyclableSerializable
    {
        public NetString()
        {
        }
        
        public NetString(string inString)
        {
            String = inString;
        }
        
        public string String;
        public override void Serialize(WriteStream writer)
        {
            writer.Write(String.Length);
            foreach (var c in String)
            {
                writer.Write(c);
            }
        }

        private static StringBuilder StringBuilder = new StringBuilder(2048);
        public override void Deserialize(ReadStream reader)
        {
            StringBuilder.Clear();
            var length = reader.ReadInt32();
            for (var i = 0; i < length; ++i)
            {
                StringBuilder.Append(reader.ReadChar());
            }

            String = StringBuilder.ToString();
        }
    }
}