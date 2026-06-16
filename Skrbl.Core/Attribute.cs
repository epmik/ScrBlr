
using System.Runtime.InteropServices;

namespace Skrbl
{
    public class Attribute
    {
        public string Key { get; protected set; } = string.Empty;

        public int Count { get; protected set; }

        public bool Normalized { get; protected set; }

        public Byte[] Array { get; private set; }

        public AttributeType Type { get; protected set; } = AttributeType.Float;

        public Attribute(string key, AttributeType attributeType, byte[] array, int count)
        {
            Type = attributeType;
            Key = key;
            Array = array;
            Count = count;
        }

        public Attribute(string key, int[] array, int count)
        {
            Type = AttributeType.Int32;
            Key = key;
            Array = MemoryMarshal.AsBytes(array.AsSpan()).ToArray();
            Count = count;
        }

        public Attribute(string key, uint[] array, int count)
        {
            Type = AttributeType.UInt32;
            Key = key;
            Array = MemoryMarshal.AsBytes(array.AsSpan()).ToArray();
            Count = count;
        }

        public Attribute(string key, float[] array, int count)
        {
            Type = AttributeType.Float;
            Key = key;
            Array = MemoryMarshal.AsBytes(array.AsSpan()).ToArray();
            Count = count;
        }
    }
}
