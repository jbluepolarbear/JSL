using JSL.Buffers;

namespace JSL.NetTypes
{
    public class NetVector2: NetRecyclableSerializable
    {
        private const float Minimum = -131072.0f;
        private const float Maximum = +131071.0f;
        private const float Scale = 262143.0f;
        private const float InverseScale = 1.0f / Scale;
        
        public override void Serialize(WriteStream writer)
        {
            var normalA = (X - Minimum) / (Maximum - Minimum);
            var normalB = (Y - Minimum) / (Maximum - Minimum);

            var integerA = (uint) (normalA * Scale + 0.5f);
            var integerB = (uint) (normalB * Scale + 0.5f);
            var remainderA = integerA >> 16;
            var remainderB = integerB >> 16;
            var remainder = remainderA << 4;
            remainder |= remainderB << 2;
            writer.Write((ushort)integerA);
            writer.Write((ushort)integerB);
            writer.Write((byte)remainder);
        }

        public override void Deserialize(ReadStream reader)
        {
            var integerA = (uint)reader.ReadUInt16();
            var integerB = (uint)reader.ReadUInt16();
            var remainder = (uint)reader.ReadByte();
            var remainderA = (remainder >> 4) & 0b_0000_0011;
            var remainderB = (remainder >> 2) & 0b_0000_0011;
            integerA |= remainderA << 16;
            integerB |= remainderB << 16;
            
            
            X = integerA * InverseScale * (Maximum - Minimum) + Minimum;
            Y = integerB * InverseScale * (Maximum - Minimum) + Minimum;
        }

        public float X;
        public float Y;
    }
}