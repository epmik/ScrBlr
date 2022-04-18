using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public class CircleGeometry : AbstractGeometry<CircleGeometry>
    {
        public const float DefaultRadius = 0.5f;
        public const int DefaultSegments = 24;

        private float _radius = DefaultRadius;
        private int _segments = DefaultSegments;

        public CircleGeometry Radius(float radius)
        {
            _radius = radius;
            
            return this;
        }

        public CircleGeometry Segments(int segments)
        {
            if(segments < 3)
            {
                throw new ArgumentOutOfRangeException("segments");
            }

            _segments = segments;

            return this;
        }

        public CircleGeometry AutoSegments(bool autoSegments)
        {
            _segments = autoSegments ? 0 : _segments;

            return this;
        }

        public CircleGeometry()
            : this(DefaultRadius)
        {

        }

        public CircleGeometry(float radius)
            : this(radius, Matrix4.Identity)
        {

        }

        public CircleGeometry(Matrix4 modelMatrix)
            : this(DefaultRadius, modelMatrix)
        {

        }

        public CircleGeometry(float radius, Matrix4 modelMatrix)
            : base(GeometryType.TriangleFan, 0, modelMatrix)
        {
            _radius = radius;
        }

        /// <summary>
        /// When AutoSegments is set to true, then VertexCount is determined by the screen resolution
        /// </summary>
        public override int VertexCount(Matrix4 modelMatrix, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            return (_segments == 0 ? DefaultSegments : _segments) + 2;
        }

        private float[,] CalculatePoints()
        {
            var segments = _segments == 0 ? DefaultSegments : _segments;

            // we're creating a triangle fan
            // +2 for the center point and duplicated endpoint
            var points = new float[segments + 2, 3];

            // center point
            points[0, 0] = _position.X;
            points[0, 1] = _position.Y;
            points[0, 2] = _position.Z;

            var step = (float)(Math.PI * 2 / (double)segments);
            var radians = 0f;

            for (var i = 1; i < segments + 1; i++)
            {
                points[i, 0] = (float)(Math.Sin(radians) * _radius);
                points[i, 1] = (float)(Math.Cos(radians) * _radius);
                points[i, 2] = 0f;

                radians += step;
            }

            // close last triangle, duplicate first point on circle
            points[segments + 1, 0] = points[1, 0];
            points[segments + 1, 1] = points[1, 1];
            points[segments + 1, 2] = points[1, 2];

            return points;
        }

        private float[,] CalculateUvs()
        {
            var segments = _segments == 0 ? DefaultSegments : _segments;

            var radius = 0.5;

            // we're creating a triangle fan
            // +2 for the center point and duplicated endpoint
            var uvs = new float[segments + 2, 2];

            var step = (float)(Math.PI * 2 / (double)segments);
            var radians = 0f;

            // center point
            uvs[0, 0] = 0.5f;
            uvs[0, 1] = 0.5f;

            for (var i = 1; i < segments + 1; i++)
            {
                uvs[i, 0] = (float)((Math.Sin(radians) * radius) + 0.5);
                uvs[i, 1] = (float)((Math.Cos(radians) * radius) + 0.5);

                radians += step;
            }

            // close last triangle, duplicate first point on circle
            uvs[segments + 1, 0] = uvs[1, 0];
            uvs[segments + 1, 1] = uvs[1, 1];

            return uvs;
        }

        public override void WriteToVertexBuffer(VertexBuffer vertexBuffer)
        {
            var points = CalculatePoints();
            var uvs = CalculateUvs();

            for (var i = 0; i < points.GetLength(0); i++)
            {
                vertexBuffer.WriteFixed(VertexFlag.Position0, new float[] { points[i, 0], points[i, 1], points[i, 2] });

                vertexBuffer.WriteFixed(VertexFlag.Normal0, DefaultNormal);

                vertexBuffer.WriteFixed(VertexFlag.Color0, ref _color);

                vertexBuffer.WriteFixed(VertexFlag.Uv0, new float[] { uvs[i, 0], uvs[i, 1] });

                vertexBuffer.WriteFixed(VertexFlag.Uv1, new float[] { uvs[i, 0], uvs[i, 1] });

                vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);
            }
        }
    }
}