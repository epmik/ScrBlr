using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public class Geometry : AbstractGeometry<Geometry>
    {
        #region Fields and Properties

        private Vertex[] _vertices;

        public static readonly int DefaultVertexCount = 1024 * 32;

        public int VertexSize { get { return _vertices.Length; } }

        public int _vertexCount;

        #endregion Fields and Properties

        #region Constructors

        public Geometry(GeometryType geometryType)
            : this(geometryType, DefaultVertexCount, Matrix4.Identity)
        {
        }

        public Geometry(GeometryType geometryType, Matrix4 modelMatrix)
            : this(geometryType, DefaultVertexCount, modelMatrix)
        {
        }

        public Geometry(GeometryType geometryType, int vertexCount, Matrix4 modelMatrix)
            : base(geometryType, DefaultVertexCount, modelMatrix)
        {
            _vertices = new Vertex[vertexCount];
        }

        #endregion Constructors

        public Vertex[] Vertices()
        {
            return _vertices;
        }

        public Vertex Vertex()
        {
            if (_vertexCount >= _vertices.Length)
            {
                throw new InvalidOperationException($"Vertex(float x, float y) failed. _vertices.Length had been reached: {_vertexCount}");
            }

            var v = new Vertex(VertexFlags);

            _vertices[_vertexCount++] = v;

            return v;
        }

        public Vertex Vertex(float x, float y, float z = 0f)
        {
            return Vertex().Set(VertexFlag.Position0, x, y, z);
        }

        public override void WriteToVertexBuffer(VertexBuffer vertexBuffer)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            base.Dispose();

            _vertices = null;
        }
    }
}