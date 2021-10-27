using System;
using JSL.Buffers;
using JSL.Utility;

namespace JSL.NetTypes
{
    public class NetCompressedVector3: NetRecyclableSerializable
    {
        private const float MinimumX = -256.0f;
        private const float MaximumX = 255.0f;
        private const float MinimumY = -256.0f;
        private const float MaximumY = 255.0f;
        private const float MinimumZ = -256.0f;
        private const float MaximumZ = 255.0f;
        private const int BitsX = 18;
        private const int BitsY = 18;
        private const int BitsZ = 14;
        private const float ScaleX = (float) ((1u << BitsX) - 1u);
        private const float ScaleY = (float) ((1u << BitsY) - 1u);
        private const float ScaleZ = (float) ((1u << BitsZ) - 1u);
        private const float InverseScaleX = 1.0f / ScaleX;
        private const float InverseScaleY = 1.0f / ScaleY;
        private const float InverseScaleZ = 1.0f / ScaleZ;

        public NetCompressedVector3()
        {
            
        }

        public void Load()
        {
            var normalA = (X - MinimumX) / (MaximumX - MinimumX);
            var normalB = (Y - MinimumY) / (MaximumY - MinimumY);
            var normalC = (Z - MinimumZ) / (MaximumZ - MinimumZ);

            _integerA = (uint) (float) Math.Floor(normalA * ScaleX + 0.5f);
            _integerB = (uint) (float) Math.Floor(normalB * ScaleY + 0.5f);
            _integerC = (uint) (float) Math.Floor(normalC * ScaleZ + 0.5f);
        }

        public void Save()
        {
            X = _integerA * InverseScaleX * (MaximumX - MinimumX) + MinimumX;
            Y = _integerB * InverseScaleY * (MaximumY - MinimumY) + MinimumY;
            Z = _integerC * InverseScaleZ * (MaximumZ - MinimumZ) + MinimumZ;
        }
        
        public override void Serialize(WriteStream writer)
        {
            Load();
            writer.WriteBits(_integerA, BitsX);
            writer.WriteBits(_integerB, BitsY);
            writer.WriteBits(_integerC, BitsZ);
        }

        public override void Deserialize(ReadStream reader)
        {
            _integerA = reader.ReadBits(BitsX);
            _integerB = reader.ReadBits(BitsY);
            _integerC = reader.ReadBits(BitsZ);
            Save();
        }

        public float X;
        public float Y;
        public float Z;
        
        private uint _integerA;
        private uint _integerB;
        private uint _integerC;
    }
}