using Skrbl;

namespace Skrbl
{
    public class Primitive
    {
        public Material Material { get; private set; }

        public PrimitiveType Type { get; private set; } = PrimitiveType.Triangles;

        public List<Attribute> Attributes { get; private set; } = new List<Attribute>();

        public PrimitiveIndexType IndexType { get; private set; } = PrimitiveIndexType.UInt32;

        public uint[]? Index { get; private set; }

        public Primitive(PrimitiveType primitiveType)
            : this (primitiveType, Material.Default)
        {
        }

        public Primitive(PrimitiveType primitiveType, Material material)
        {
            Type = primitiveType;
            Material = material;
        }

        public void AddAttribute(Attribute attribute)
        {
            Attributes.RemoveAll(o => o.Key == attribute.Key);

            Attributes.Add(attribute);
        }

        public void AddIndex(IEnumerable<uint> index)
        {
            Index = [.. index];
        }

        //public void AddIndex(IEnumerable<int> index)
        //{
        //    Index = index.Select(o => (uint)o).ToArray();
        //}

        //public void AddIndex(IEnumerable<ushort> index)
        //{
        //    Index = index.Select(o => (uint)o).ToArray();
        //}

        //public void AddIndex(IEnumerable<short> index)
        //{
        //    Index = index.Select(o => (uint)o).ToArray();
        //}
    }
}
