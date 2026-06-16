using System;
using System.Collections.Generic;
using System.Text;

namespace Skrbl
{
    public class Mesh
    {
        public Transform Transform { get; private set; } = new Transform();

        public Material? Material { get; private set; }

        //public List<Primitive> Primitives { get; set; } = new List<Primitive>();

        public int Count { get; private set; }

        public PrimitiveType Type { get; private set; } = PrimitiveType.Triangles;

        public List<Attribute> Attributes { get; private set; } = new List<Attribute>();

        public PrimitiveIndexType IndexType { get; private set; } = PrimitiveIndexType.UInt32;

        public uint[]? Index { get; private set; }

        public List<Mesh> Children { get; private set; } = new List<Mesh>();

        public Mesh()
            : this(0)
        {
        }

        public Mesh(int count)
            : this(count, PrimitiveType.Triangles)
        {
        }

        public Mesh(int count, PrimitiveType primitiveType)
            : this(count, primitiveType, null)
        {
        }

        public Mesh(int count, PrimitiveType primitiveType, Material? material)
        {
            Count = count;
            Type = primitiveType;
            Material = material;
        }

        public void AddChild(Mesh mesh)
        {
            Children.Add(mesh);
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
    }
}
