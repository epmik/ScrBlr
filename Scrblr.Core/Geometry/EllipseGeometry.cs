using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public class EllipseGeometry : AbstractGeometry<EllipseGeometry>
    {
        // TODO allow for different radius left/right and top/bottom
        public const float DefaultWidth = 1f;
        public const float DefaultHeight = 0.5f;
        public const int DefaultSegments = 24;

        private float _width = DefaultWidth;
        private float _height = DefaultHeight;
        private int _segments = DefaultSegments;

        public EllipseGeometry Width(float width)
        {
            _width = width;
            
            return this;
        }

        public EllipseGeometry Height(float height)
        {
            _height = height;

            return this;
        }

        public EllipseGeometry Segments(int segments)
        {
            if(segments < 3)
            {
                throw new ArgumentOutOfRangeException("segments");
            }

            _segments = segments;

            return this;
        }

        public EllipseGeometry AutoSegments(bool autoSegments)
        {
            _segments = autoSegments ? 0 : _segments;

            return this;
        }

        public EllipseGeometry()
            : this(DefaultWidth, DefaultHeight)
        {

        }

        public EllipseGeometry(float width, float height)
            : this(width, height, Matrix4.Identity)
        {

        }

        public EllipseGeometry(Matrix4 modelMatrix)
            : this(DefaultWidth, DefaultHeight, modelMatrix)
        {

        }

        public EllipseGeometry(float width, float height, Matrix4 modelMatrix)
            : base(GeometryType.TriangleFan, 0, modelMatrix)
        {
            _width = width;
            _height = height;
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

            var radiusW = _width * 0.5;
            var radiusH = _height * 0.5;

            // we're creating a triangle fan
            // +2 for the center point and duplicated endpoint
            var points = new float[segments + 2, 3];

            // center point
            points[0, 0] = _position.X;
            points[0, 1] = _position.Y;
            points[0, 2] = _position.Z;

            var step = Math.PI * 2 / (double)segments;
            var radians = Math.PI * 0.5;

            // alternate method https://stackoverflow.com/questions/10322341/simple-algorithm-for-drawing-filled-ellipse-in-c-c
            // https://web.archive.org/web/20120225095359/http://homepage.smc.edu/kennedy_john/belipse.pdf
            // https://github.com/notprathap/filled-ellipse
            // https://github.com/notprathap/filled-ellipse/releases/tag/filled-ellipse-with-triangles

            for (var i = 1; i < segments + 1; i++)
            {
                // see https://math.stackexchange.com/a/396533/1049098
                points[i, 0] = (float)(Math.Cos(radians) * radiusW);
                points[i, 1] = (float)(Math.Sin(radians) * radiusH);
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
            // TODO allow different uv generation
            // now uv's are distorted/stretched to fit within the ellipse bounds: allow for non distorted/stretched uv's,
            // allow for repeating uv's when width and height are greater then 1f

            var segments = _segments == 0 ? DefaultSegments : _segments;

            var halfRadius = 0.5;

            // we're creating a triangle fan
            // +2 for the center point and duplicated endpoint
            var uvs = new float[segments + 2, 2];

            var step = Math.PI * 2 / (double)segments;
            var radians = Math.PI * 0.5;

            // center point
            uvs[0, 0] = 0.5f;
            uvs[0, 1] = 0.5f;

            for (var i = 1; i < segments + 1; i++)
            {
                uvs[i, 0] = (float)((Math.Cos(radians) * halfRadius) + 0.5);
                uvs[i, 1] = (float)((Math.Sin(radians) * halfRadius) + 0.5);

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