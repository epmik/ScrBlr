using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public class QuadGeometry : AbstractGeometry<QuadGeometry>
    {
        private float[,] _points = new float[4,3]
        {
            // top left
            {  -0.5f,  0.5f,  0.0f },
            // bottom left
            {  -0.5f, -0.5f,  0.0f },
            // top right
            {   0.5f,  0.5f,  0.0f },
            // bottom right
            {   0.5f, -0.5f,  0.0f },
        };

        private float[,] _uvs;

        private static readonly float[,] _defaultUvs = new float[4,2]
            {
                // top left
                { 0f, 1f },
                // bottom left
                { 0, 0f },
                // top right
                { 1f, 1f },
                // bottom right
                { 1f, 0f },
            };

        public QuadGeometry()
            : base(GeometryType.TriangleStrip, 4, Matrix4.Identity)
        {

        }

        public QuadGeometry(Matrix4 modelMatrix)
            : base(GeometryType.TriangleStrip, 4, modelMatrix)
        {

        }

        public QuadGeometry Points(ref float[,] points)
        {
            var rank = points.Length == 8 ? 2 : 3;

            // TODO quicker copy?
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < rank; j++)
                {
                    _points[i,j] = points[i,j];
                }
            }

            return this;
        }

        public QuadGeometry Points(ref float[] points)
        {
            var rank = points.Length == 8 ? 2 : 3;

            // TODO quicker copy?
            var k = 0;
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < rank; j++)
                {
                    _points[i,j] = points[k++];
                }
            }

            return this;
        }

        public QuadGeometry Points(float[,] points)
        {
            return Points(ref points);
        }

        public QuadGeometry Points(float[] points)
        {
            return Points(ref points);
        }

        public QuadGeometry Uvs(ref float[,] uvs)
        {
            if(_uvs == null)
            {
                // TODO copy _defaultUvs
                _uvs = new float[4,2]
                {
                        // top left
                        { 0f, 1f },
                        // bottom left
                        { 0, 0f },
                        // top right
                        { 1f, 1f },
                        // bottom right
                        { 1f, 0f },
                };
            }

            // TODO quicker copy?
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    _uvs[i,j] = uvs[i,j];
                }
            }

            return this;
        }

        public QuadGeometry Uvs(ref float[] uvs)
        {
            if (_uvs == null)
            {
                // TODO copy _defaultUvs
                _uvs = new float[4, 2]
                {
                        // top left
                        { 0f, 1f },
                        // bottom left
                        { 0, 0f },
                        // top right
                        { 1f, 1f },
                        // bottom right
                        { 1f, 0f },
                };
            }

            // TODO quicker copy?
            var k = 0;
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    _uvs[i,j] = uvs[k++];
                }
            }

            return this;
        }

        public QuadGeometry Uvs(float[,] uvs)
        {
            return Uvs(ref uvs);
        }

        public QuadGeometry Uvs(float[] uvs)
        {
            return Uvs(ref uvs);
        }

        public override void WriteToVertexBuffer(VertexBuffer vertexBuffer)
        {
            for (var i = 0; i < 4; i++)
            {
                vertexBuffer.WriteFixed(VertexFlag.Position0, new float[] { _points[i, 0], _points[i, 1], _points[i, 2] });

                vertexBuffer.WriteFixed(VertexFlag.Normal0, DefaultNormal);

                vertexBuffer.WriteFixed(VertexFlag.Color0, ref _color);

                vertexBuffer.WriteFixed(VertexFlag.Uv0, (_uvs != null ? new float[] { _uvs[i, 0], _uvs[i, 1] } : new float[] { _defaultUvs[i, 0], _defaultUvs[i, 1] }));

                vertexBuffer.WriteFixed(VertexFlag.Uv1, (_uvs != null ? new float[] { _uvs[i, 0], _uvs[i, 1] } : new float[] { _defaultUvs[i, 0], _defaultUvs[i, 1] }));

                vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);
            }
        }
    }
}