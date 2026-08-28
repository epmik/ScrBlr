namespace Scrbl.JaccoBikker
{
    using System;

    public class Scene
    {

        public uint TriangleCount { get; set; }

        public Triangle[] Triangles { get; set; }

        public uint[] TriangleIndices { get; set; }
    }
}
