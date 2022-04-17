using OpenTK.Mathematics;
using System;

namespace Scrblr.Core
{
    public class Geometry20220413 : IDisposable
    {
        private static int DefaultRgba = System.Drawing.Color.FromArgb(0, 0, 0, 0).ToArgb();

        public struct V
        {
            public float X;
            public float Y;
            public float Z;

            public float W;
            
            public int Rgba;

            public V(float x, float y, float z = 0)
            {
                X = x;
                Y = y;
                Z = z;

                W = 0;

                Rgba = DefaultRgba;
            }

            public V Color(int r, int g, int b, int a = 255)
            {
                Rgba = System.Drawing.Color.FromArgb(a, r, g, b).ToArgb();

                return this;
            }

            public V Color(Color4 color)
            {
                return Color(color.R, color.G, color.B, color.A);
            }

            public V Color(float r, float g, float b, float a = 1f)
            {
                return Color((int)(r * 255), (int)(g * 255), (int)(b * 255), (int)(a * 255));
            }

            public V Weight(float weight)
            {
                W = weight;

                return this;
            }
        }

        public GeometryType20220413 GeometryType { get; private set; } = GeometryType20220413.Triangles;

        private V[] _vertices;

        private int _vertexCount = 0;

        private Shader20220413 _shader;

        private Matrix4 _matrix;

        public Geometry20220413(GeometryType20220413 geometryType, Shader20220413 shader, Matrix4 matrix)
        {
            GeometryType = geometryType;
            _vertices = new V[1024 * 1024];
            _shader = shader;
            _matrix = matrix;
        }

        public void Dispose()
        {
            _vertices = null;
        }

        //public static Geometry Shape(GeometryKind shapeKind)
        //{
        //    return new Geometry(shapeKind);
        //}

        public V Vertex(float x, float y)
        {
            if (_vertexCount + 1 >= _vertices.Length)
            {
                throw new InvalidOperationException($"Vertex(float x, float y) failed. _vertices.Length had been reached: {_vertexCount}");
            }

            var v = new V();
            
            v.X = x;
            v.Y = y;

            _vertices[_vertexCount++] = v;

            return v;
        }
    }
}