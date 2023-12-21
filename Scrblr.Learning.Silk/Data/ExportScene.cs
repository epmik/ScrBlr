using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;

namespace Scrblr.Learning
{
    public class ExportScene
    {
        public enum Flag
        {
            Vertices,
            Normals,
            Uv0,
        }

        public class Setting
        {
            public bool Indexed;

            public List<Flag> Flags = new List<Flag> { Flag.Vertices };
        }

        public class Mesh
        {
            public string Name;
            public float[] Vertices;
            public uint VertexCount;
            public uint VertexSize;
            public uint[] Indices;
        }

        public Setting Settings;
        public Mesh[] Meshes;
    }
}
