using OpenTK.Graphics.OpenGL4;
using System;

namespace Scrblr.Core
{
    public enum GeometryType20220413
    {
        Points = PrimitiveType.Points,
        Triangles = PrimitiveType.Triangles,
        TriangleStrip = PrimitiveType.TriangleStrip,
        TriangleFan = PrimitiveType.TriangleFan,
        Lines = PrimitiveType.Lines,
        LineLoop = PrimitiveType.LineLoop,
        LineStrip = PrimitiveType.LineStrip,

        Vertex,
        VertexStrip,
        VertexFan,
    }
}