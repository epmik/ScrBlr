using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public class LineGeometry : AbstractGeometry<LineGeometry>
    {
        public const float DefaultWidth = 1f;

        private Color4 _currentColor = new Color4(0f, 0f, 0f, 1f);
        private float _currentWidth = 0.1f;

        private enum ConnectionType
        {
            From,
            To,
        }

        private class Connection
        {
            public Vector3 Point;
            public Color4 Color;
            public float Width;
            public ConnectionType ConnectionType;
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

            _connections.Add(new Connection { Point = new Vector3(0f, 0f, 0f), Color = _currentColor, Width = _currentWidth, ConnectionType = ConnectionType.From });
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

        public override RenderBatch[] ToRenderBatch(GraphicsContext graphicsContext, GraphicsState graphicsState, Shader shader, VertexBuffer vertexBuffer, ICamera camera)
        {
            var normal = Vector3.UnitZ;

            var modelMatrix = ModelMatrix();
            var viewMatrix = camera.ViewMatrix();
            var projectionMatrix = camera.ProjectionMatrix();

            var renderBatches = new List<RenderBatch>();

            var vertexCount = 0;

            for (var i = 0; i < _connections.Count - 1; i++)
            {
                var from = _connections[i];
                var to = _connections[i + 1];

                if (from.ConnectionType == ConnectionType.From && to.ConnectionType == ConnectionType.From)
                {
                    continue;
                }

                var direction = to.Point - from.Point;

                var forward = normal;

                var right = Vector3.Normalize(Vector3.Cross(direction, forward));

                var offsetFrom = right * from.Width * 0.5f;

                var fromLeft = from.Point - offsetFrom;
                var fromright = from.Point + offsetFrom;

                var offsetTo = right * to.Width * 0.5f;

                var toLeft = to.Point - offsetTo;
                var toRight = to.Point + offsetTo;

                vertexBuffer.WriteFixed(VertexFlag.Position0, fromright.X, fromright.Y, fromright.Z);
                vertexBuffer.WriteFixed(VertexFlag.Normal0, forward.X, forward.Y, forward.Z);
                vertexBuffer.WriteFixed(VertexFlag.Color0, from.Color.R, from.Color.G, from.Color.B, from.Color.A);
                vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);

                vertexBuffer.WriteFixed(VertexFlag.Position0, fromLeft.X, fromLeft.Y, fromLeft.Z);
                vertexBuffer.WriteFixed(VertexFlag.Normal0, forward.X, forward.Y, forward.Z);
                vertexBuffer.WriteFixed(VertexFlag.Color0, from.Color.R, from.Color.G, from.Color.B, from.Color.A);
                vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);

                vertexBuffer.WriteFixed(VertexFlag.Position0, toLeft.X, toLeft.Y, toLeft.Z);
                vertexBuffer.WriteFixed(VertexFlag.Normal0, forward.X, forward.Y, forward.Z);
                vertexBuffer.WriteFixed(VertexFlag.Color0, to.Color.R, to.Color.G, to.Color.B, to.Color.A);
                vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);

                vertexBuffer.WriteFixed(VertexFlag.Position0, fromright.X, fromright.Y, fromright.Z);
                vertexBuffer.WriteFixed(VertexFlag.Normal0, forward.X, forward.Y, forward.Z);
                vertexBuffer.WriteFixed(VertexFlag.Color0, from.Color.R, from.Color.G, from.Color.B, from.Color.A);
                vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);

                vertexBuffer.WriteFixed(VertexFlag.Position0, toLeft.X, toLeft.Y, toLeft.Z);
                vertexBuffer.WriteFixed(VertexFlag.Normal0, forward.X, forward.Y, forward.Z);
                vertexBuffer.WriteFixed(VertexFlag.Color0, to.Color.R, to.Color.G, to.Color.B, to.Color.A);
                vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);

                vertexBuffer.WriteFixed(VertexFlag.Position0, toRight.X, toRight.Y, toRight.Z);
                vertexBuffer.WriteFixed(VertexFlag.Normal0, forward.X, forward.Y, forward.Z);
                vertexBuffer.WriteFixed(VertexFlag.Color0, to.Color.R, to.Color.G, to.Color.B, to.Color.A);
                vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);

                vertexCount += 6;

                if (i + 2 == _connections.Count || _connections[i + 2].ConnectionType == ConnectionType.From)
                {
                    renderBatches.Add(new RenderBatch
                    {
                        State = graphicsState,
                        Shader = shader,
                        VertexBuffer = vertexBuffer,
                        ViewMatrix = viewMatrix,
                        ViewPosition = camera.Position,
                        ProjectionMatrix = projectionMatrix,
                        ModelMatrix = modelMatrix,
                        GeometryType = GeometryType.Triangles,
                        ElementCount = vertexCount,
                        ElementIndex = vertexBuffer.UsedElements() - vertexCount,
                        Texture0 = _texture0,
                        Texture1 = _texture1,
                        Texture2 = _texture2,
                        VertexFlags = VertexFlags,
                    });

                    vertexCount = 0;
                }
            }

            return renderBatches.ToArray();
        }

        public override void WriteToVertexBuffer(VertexBuffer vertexBuffer)
        {
        }

        public LineGeometry From(float x, float y, float z = 0f)
        {
            if(_connections[_connections.Count - 1].ConnectionType == ConnectionType.From)
            {
                _connections.RemoveAt(_connections.Count - 1);
            }

            _connections.Add(new Connection { Point = new Vector3(x, y, z), Color = _currentColor, Width = _currentWidth, ConnectionType = ConnectionType.From });

            return this;
        }

        public LineGeometry To(float x, float y, float z = 0f)
        {
            _connections.Add(new Connection { Point = new Vector3(x, y, z), Color = _currentColor, Width = _currentWidth, ConnectionType = ConnectionType.To });

            return this;
        }

        public override LineGeometry Color(float r, float g, float b, float a = 1f)
        {
            _currentColor.R = r;
            _currentColor.G = g;
            _currentColor.B = b;
            _currentColor.A = a;

            return this;
        }

        public LineGeometry Width(float w)
        {
            _currentWidth = w;

            return this;
        }

        public LineGeometry ToFirst()
        {
            var firstPoint = _connections[0].Point;
            var firstColor = _connections[0].Color;
            var firstWidth = _connections[0].Width;

            _connections.Add(new Connection { Point = firstPoint, Color = firstColor, Width = firstWidth, ConnectionType = ConnectionType.To });

            return this;
        }
    }
}