using OpenTK.Graphics.OpenGL4;
using System;

namespace Scrblr.Core
{
    public enum GeometryType
    {
        Points = PrimitiveType.Points,
        Triangles = PrimitiveType.Triangles,
        TriangleStrip = PrimitiveType.TriangleStrip,
        TriangleFan = PrimitiveType.TriangleFan,
        Lines = PrimitiveType.Lines,
        LineLoop = PrimitiveType.LineLoop,
        LineStrip = PrimitiveType.LineStrip,

        //Vertex = 1000,
        //VertexStrip = 1001,
        //VertexFan = 1002,
        //VertexLoop = 1003,
    }
}