using JSL.Buffers;
using JSL.Utility;

namespace JSL.NetTypes
{
    public class NetVector3: NetRecyclable
    {
        private const float MinimumX = -256.0f;
        private const float MaximumX = 255.0f;
        private const float MinimumY = 0.0f;
        private const float MaximumY = 75.0f;
        private const float MinimumZ = -256.0f;
        private const float MaximumZ = 255.0f;

        public NetVector3()
        {
            
        }
        
        public override void Serialize(WriteStream writer)
        {
            var normalA = (X - MinimumX) / (MaximumX - MinimumX);
            var normalB = (Y - MinimumY) / (MaximumY - MinimumY);
            var normalC = (Z - MinimumZ) / (MaximumZ - MinimumZ);

            var integerA = (uint) (normalA * (MathHelpers.PowerOf2(18) - 1.0f) + 0.5f);
            var integerB = (uint) (normalB * (MathHelpers.PowerOf2(14) - 1.0f) + 0.5f);
            var integerC = (uint) (normalC * (MathHelpers.PowerOf2(18) - 1.0f) + 0.5f);
            
            writer.WriteBits(integerA, 18);
            writer.WriteBits(integerB, 14);
            writer.WriteBits(integerC, 18);
        }

        public override void Deserialize(ReadStream reader)
        {
            var integerA = reader.ReadBits(18);
            var integerB = reader.ReadBits(14);
            var integerC = reader.ReadBits(18);

            X = integerA * (1.0f / (MathHelpers.PowerOf2(18) - 1.0f)) * (MaximumX - MinimumX) + MinimumX;
            Y = integerB * (1.0f / (MathHelpers.PowerOf2(14) - 1.0f)) * (MaximumY - MinimumY) + MinimumY;
            Z = integerC * (1.0f / (MathHelpers.PowerOf2(18) - 1.0f)) * (MaximumZ - MinimumZ) + MinimumZ;
        }

        public float X;
        public float Y;
        public float Z;
    }
}