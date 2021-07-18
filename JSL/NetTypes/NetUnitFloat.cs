using System;
using JSL.Buffers;

namespace JSL.NetTypes
{
    /// <summary>
    /// Float with range from -1 to 1 inclusive
    /// </summary>
    public class NetUnitFloat: NetRecyclable
    {
        private const float Minimum = -1.0f / 1.414214f; // 1.0 / sqrt(2)
        private const float Maximum = +1.0f / 1.414214f;

        private const float Scale = 65535.0f; // 2 ^ 10 - 1
        private const float InverseScale = 1.0f / Scale;
        
        public override void Serialize(WriteStream writer)
        {
            var normal = (_value - Minimum) / (Maximum - Minimum);
            var integer = (ushort) (normal * Scale + 0.5f);
            writer.Write(integer);
        }

        public override void Deserialize(ReadStream reader)
        {
            var integer = reader.ReadUInt16();
            _value = integer * InverseScale * (Maximum - Minimum) + Minimum;
        }

        private float _value;

        public float Value
        {
            get => _value;
            set => _value = Math.Max(Math.Min(1.0f, value), -1.0f);
        }
    }
}