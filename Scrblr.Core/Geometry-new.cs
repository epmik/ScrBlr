using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public class Geometry : AbstractGeometry
    {
        #region Fields and Properties

        private Vertex[] _vertices;

        public static readonly int DefaultVertexCount = 1024 * 32;

        private int _vertexIndex;

        /// <summary>
        /// Returns the number of vertices in use. this is not the number of allocated vertices
        /// </summary>
        public override int VertexCount { get { return _vertexIndex; } }

        //private int _vertexArraySize { get { return _vertices.Length; } }

        #endregion Fields and Properties

        #region Constructors

        public Geometry(GeometryType geometryType, Shader shader, Matrix4 modelMatrix)
            : this(geometryType, DefaultVertexCount, shader, modelMatrix)
        {
        }

        public Geometry(GeometryType geometryType, int vertexCount, Shader shader, Matrix4 modelMatrix)
            : base(geometryType, DefaultVertexCount, shader, modelMatrix)
        {
            _vertices = new Vertex[VertexCount];
        }

        #endregion Constructors

        public Vertex[] Vertices()
        {
            return _vertices;
        }

        public Vertex Vertex()
        {
            if (_vertexIndex >= _vertices.Length)
            {
                throw new InvalidOperationException($"Vertex(float x, float y) failed. _vertices.Length had been reached: {_vertexIndex}");
            }

            var v = new Vertex(VertexFlags);

            _vertices[_vertexIndex++] = v;

            return v;
        }

        public Vertex Vertex(float x, float y, float z = 0f)
        {
            return Vertex().Set(VertexFlag.Position0, x, y, z);
        }

        public class QuadGeometry : AbstractGeometry, IGeometry
        {
            /// <summary>
            /// default == 1f
            /// </summary>
            public float _width = 1f;

            /// <summary>
            /// default == 1f
            /// </summary>
            public float _height = 1f;

            public QuadGeometry(Shader shader, Matrix4 modelMatrix)
                : base(GeometryType.TriangleStrip, 4, shader, modelMatrix)
            {
                
            }

            public QuadGeometry Width(float w)
            {
                _width = w;

                return this;
            }

            public QuadGeometry Height(float h)
            {
                _height = h;

                return this;
            }

            private float[][] Points()
            {
                var hw = _width * 0.5f;
                var hh = _height * 0.5f;

                // ccw triangle order
                return new float[][]
                {
                    // top left
                    new float[] { _position.X + hw, _position.Y + hh, 0f },
                    // bottom left
                    new float[] { _position.X + hw, _position.Y - hh, 0f },
                    // top right
                    new float[] { _position.X - hw, _position.Y + hh, 0f },
                    // bottom right
                    new float[] { _position.X - hw, _position.Y - hh, 0f },
                };
            }

            private float[][] Uvs()
            {
                return new float[][]
                {
                    // top left
                    new float[] { 0f, 0f },
                    // bottom left
                    new float[] { 0, 1f },
                    // top right
                    new float[] { 1f, 0f },
                    // bottom right
                    new float[] { 1f, 1f },
                };
            }

            #region Overrides

            public QuadGeometry Color(float r, float g, float b, float a = 1)
            {
                return base.Color<QuadGeometry>(r, g, b, a);
            }

            public QuadGeometry Color(float grey, float a = 1)
            {
                return base.Color<QuadGeometry>(grey, a);
            }

            #endregion

            public static void WriteTo(Geometry.QuadGeometry quad, VertexBuffer vertexBuffer)
            {
                var points = quad.Points();
                var uvs = quad.Uvs();

                for (var i = 0; i < 4; i++)
                {
                    vertexBuffer.WriteFixed(VertexFlag.Position0, points[i]);
                    
                    vertexBuffer.WriteFixed(VertexFlag.Normal0, DefaultNormal);

                    vertexBuffer.WriteFixed(VertexFlag.Color0, ref quad._color);

                    vertexBuffer.WriteFixed(VertexFlag.Uv0, uvs[i]);

                    vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _vertices = null;
        }
    }
}