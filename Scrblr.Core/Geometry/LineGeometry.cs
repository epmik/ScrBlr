using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public class LineGeometry : AbstractGeometry<LineGeometry>
    {
        public const float DefaultWidth = 1f;

        private Color4 _defaultColor = new Color4(0f, 0f, 0f, 1f);
        private float _defaultWidth = 0.1f;

        private class Connection
        {
            public Vector3 Point;
            public Color4 Color;
            public float Width;
        }

        private List<Connection> _connections;

        //public LineGeometry Radius(float radius)
        //{
        //    _radius = radius;
            
        //    return this;
        //}

        //public LineGeometry Segments(int segments)
        //{
        //    if(segments < 3)
        //    {
        //        throw new ArgumentOutOfRangeException("segments");
        //    }

        //    _segments = segments;

        //    return this;
        //}

        //public LineGeometry AutoSegments(bool autoSegments)
        //{
        //    // TODO number of segments dependent on the size of the circle in screen space
        //    _segments = autoSegments ? 0 : _segments;

        //    return this;
        //}

        public LineGeometry()
            : this(Matrix4.Identity)
        {

        }

        public LineGeometry(Matrix4 modelMatrix)
            : base(GeometryType.TriangleStrip, 0, modelMatrix)
        {
            VertexFlags = VertexFlag.Position0 | VertexFlag.Color0;

            _connections = new List<Connection>();

            _connections.Add(new Connection { Point = new Vector3(0f, 0f, 0f), Color = _defaultColor, Width = _defaultWidth });
        }

        ///// <summary>
        ///// When AutoSegments is set to true, then VertexCount is determined by the screen resolution
        ///// </summary>
        //public override int VertexCount(Matrix4 modelMatrix, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        //{
        //    return (_segments == 0 ? DefaultSegments : _segments) + 2;
        //}

        //private float[,] CalculatePoints()
        //{
        //    var segments = _segments == 0 ? DefaultSegments : _segments;

        //    // we're creating a triangle fan
        //    // +2 for the center point and duplicated endpoint
        //    var points = new float[segments + 2, 3];

        //    // center point
        //    points[0, 0] = _position.X;
        //    points[0, 1] = _position.Y;
        //    points[0, 2] = _position.Z;

        //    var step = Math.PI * 2 / (double)segments;
        //    var radians = Math.PI * 0.5;

        //    for (var i = 1; i < segments + 1; i++)
        //    {
        //        points[i, 0] = (float)(Math.Cos(radians) * _radius);
        //        points[i, 1] = (float)(Math.Sin(radians) * _radius);
        //        points[i, 2] = 0f;

        //        radians += step;
        //    }

        //    // close last triangle, duplicate first point on circle
        //    points[segments + 1, 0] = points[1, 0];
        //    points[segments + 1, 1] = points[1, 1];
        //    points[segments + 1, 2] = points[1, 2];

        //    return points;
        //}

        //private float[,] CalculateUvs()
        //{
        //    var segments = _segments == 0 ? DefaultSegments : _segments;

        //    var radius = 0.5;

        //    // we're creating a triangle fan
        //    // +2 for the center point and duplicated endpoint
        //    var uvs = new float[segments + 2, 2];

        //    var step = Math.PI * 2 / (double)segments;
        //    var radians = Math.PI * 0.5;

        //    // center point
        //    uvs[0, 0] = 0.5f;
        //    uvs[0, 1] = 0.5f;

        //    for (var i = 1; i < segments + 1; i++)
        //    {
        //        uvs[i, 0] = (float)((Math.Cos(radians) * radius) + 0.5);
        //        uvs[i, 1] = (float)((Math.Sin(radians) * radius) + 0.5);

        //        radians += step;
        //    }

        //    // close last triangle, duplicate first point on circle
        //    uvs[segments + 1, 0] = uvs[1, 0];
        //    uvs[segments + 1, 1] = uvs[1, 1];

        //    return uvs;
        //}

        public override void WriteToVertexBuffer(VertexBuffer vertexBuffer)
        {
            //var points = CalculatePoints();
            //var uvs = CalculateUvs();

            //for (var i = 0; i < points.GetLength(0); i++)
            //{
            //    vertexBuffer.WriteFixed(VertexFlag.Position0, new float[] { points[i, 0], points[i, 1], points[i, 2] });

            //    vertexBuffer.WriteFixed(VertexFlag.Normal0, DefaultNormal);

            //    vertexBuffer.WriteFixed(VertexFlag.Color0, ref _color);

            //    vertexBuffer.WriteFixed(VertexFlag.Uv0, new float[] { uvs[i, 0], uvs[i, 1] });

            //    vertexBuffer.WriteFixed(VertexFlag.Uv1, new float[] { uvs[i, 0], uvs[i, 1] });

            //    vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);
            //}
        }

        public LineGeometry From(float x, float y, float z = 0f)
        {
            if(_connections.Count == 1)
            {
                _connections[0].Point.X = x;
                _connections[0].Point.Y = y;
                _connections[0].Point.Z = z;
            }
            else
            {
                var i = _connections.Count - 1;

                var previousColor = _connections[i].Color;
                var previousWidth = _connections[i].Width;

                _connections.Add(new Connection { Point = new Vector3(x, y, z), Color = previousColor, Width = previousWidth });
            }

            return this;
        }

        public LineGeometry To(float x, float y, float z = 0f)
        {
            var i = _connections.Count - 1;

            var previousColor = _connections[i].Color;
            var previousWidth = _connections[i].Width;

            _connections.Add(new Connection { Point = new Vector3(x, y, z), Color = previousColor, Width = previousWidth });

            return this;
        }

        public override LineGeometry Color(float r, float g, float b, float a = 1f)
        {
            if(_connections.Count == 0)
            {
                _defaultColor.R = r;
                _defaultColor.G = g;
                _defaultColor.B = b;
                _defaultColor.A = a;

                return this;
            }

            var i = _connections.Count - 1;

            _connections[i].Color.R = r;
            _connections[i].Color.G = g;
            _connections[i].Color.B = b;
            _connections[i].Color.A = a;

            return this;
        }

        public LineGeometry Width(float w)
        {
            if (_connections.Count == 0)
            {
                _defaultWidth = w;

                return this;
            }

            var i = _connections.Count - 1;

            _connections[i].Width = w;

            return this;
        }

        public LineGeometry ToFirst()
        {
            var firstPoint = _connections[0].Point;
            var firstColor = _connections[0].Color;
            var firstWidth = _connections[0].Width;

            _connections.Add(new Connection { Point = firstPoint, Color = firstColor, Width = firstWidth });

            return this;
        }
    }
}