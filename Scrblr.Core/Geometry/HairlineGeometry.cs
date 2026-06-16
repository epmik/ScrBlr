using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Scrblr.Core
{
    public class HairlineGeometry : AbstractGeometry<HairlineGeometry>
    {
        /// <summary>
        /// default == 1f
        /// </summary>
        public float _width = 1f;

        /// <summary>
        /// default == 1f
        /// </summary>
        public float _height = 1f;

        private Color4 _currentColor = new Color4(0f, 0f, 0f, 1f);

        private enum ConnectionType
        {
            From,
            To,
        }

        private class Connection
        {
            public Vector3 Point;
            public Color4 Color;
            public ConnectionType ConnectionType;
            public GeometryType GeometryType;
        }

        private List<Connection> _connections;

        public HairlineGeometry()
            : this(Matrix4.Identity)
        {

        }

        public HairlineGeometry(Matrix4 modelMatrix)
            : base(GeometryType.Lines, 0, modelMatrix)
        {
            VertexFlags = VertexFlag.Position0 | VertexFlag.Color0;

            _connections = new List<Connection>();

            _connections.Add(
                new Connection 
                { 
                    Point = new Vector3(0f, 0f, 0f), 
                    Color = _currentColor, 
                    ConnectionType = ConnectionType.From,
                    GeometryType = GeometryType.Lines,
                });

            _vertexCount++;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HairlineGeometry Size(float w, float h)
        {
            _width = w;
            _height = h;

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HairlineGeometry Size(float wh)
        {
            return Size(wh, wh);
        }

        public HairlineGeometry From(float x, float y, float z = 0f)
        {
            if (_connections[_connections.Count - 1].ConnectionType == ConnectionType.From)
            {
                _connections.RemoveAt(_connections.Count - 1);

                _vertexCount--;
            }

            _connections.Add(
                new Connection
                {
                    Point = new Vector3(x, y, z),
                    Color = _currentColor,
                    ConnectionType = ConnectionType.From,
                    GeometryType = GeometryType.Lines,
                });

            _vertexCount++;

            return this;
        }

        public HairlineGeometry To(float x, float y, float z = 0f)
        {
            var geometryType = _connections[_connections.Count - 1].ConnectionType == ConnectionType.From ? GeometryType.Lines : GeometryType.LineStrip;

            _connections.Add(new Connection { Point = new Vector3(x, y, z), Color = _currentColor, ConnectionType = ConnectionType.To, GeometryType = geometryType });

            _vertexCount++;

            return SetGeometryType(geometryType);
        }

        public HairlineGeometry Close()
        {
            if(_connections[_connections.Count - 1].ConnectionType == ConnectionType.From)
            {
                return this;
            }

            return SetGeometryType(GeometryType.LineLoop);
        }

        public HairlineGeometry SetGeometryType(GeometryType geometryType)
        {
            var i = _connections.Count - 1;
            var quit = false;

            while (!quit)
            {
                if (_connections[i].ConnectionType == ConnectionType.From)
                {
                    quit = true;
                }

                _connections[i].GeometryType = geometryType;

                i--;
            }

            return this;
        }

        public override HairlineGeometry Color(float r, float g, float b, float a = 1f)
        {
            _currentColor.R = r;
            _currentColor.G = g;
            _currentColor.B = b;
            _currentColor.A = a;

            return this;
        }

        public override RenderBatch[] ToRenderBatch(GraphicsContext graphicsContext, GraphicsState graphicsState, Shader shader, VertexBuffer vertexBuffer, ICamera camera)
        {
            var modelMatrix = ModelMatrix();
            var viewMatrix = camera.ViewMatrix();
            var projectionMatrix = camera.ProjectionMatrix();

            var renderBatches = new List<RenderBatch>();

            var vertexCount = 0;

            for (var i = 0; i < _connections.Count; )
            {
                if (_connections[i].ConnectionType == ConnectionType.From)
                {
                    if(i + 1 >= _connections.Count || _connections[i + 1].ConnectionType == ConnectionType.From)
                    {
                        i++;

                        continue;
                    }

                    var write = true;

                    var geometryType = _connections[i].GeometryType;

                    while (write)
                    {
                        vertexBuffer.WriteFixed(VertexFlag.Position0, new float[] { _connections[i].Point.X, _connections[i].Point.Y, _connections[i].Point.Z });

                        vertexBuffer.WriteFixed(VertexFlag.Color0, new float[] { _connections[i].Color.R, _connections[i].Color.G, _connections[i].Color.B, _connections[i].Color.A });

                        vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);

                        i++;

                        vertexCount++;

                        write = i < _connections.Count && _connections[i].ConnectionType != ConnectionType.From;
                    }

                    renderBatches.Add(new RenderBatch
                    {
                        State = graphicsState,
                        Shader = shader,
                        VertexBuffer = vertexBuffer,
                        ViewMatrix = viewMatrix,
                        ViewPosition = camera.Position,
                        ProjectionMatrix = projectionMatrix,
                        ModelMatrix = modelMatrix,
                        GeometryType = geometryType,
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
            //var points = CalculatePoints();

            //for (var i = 0; i < 2; i++)
            //{
            //    vertexBuffer.WriteFixed(VertexFlag.Position0, new float[] { points[i, 0], points[i, 1], points[i, 2] });

            //    //vertexBuffer.WriteFixed(VertexFlag.Normal0, DefaultNormal);

            //    vertexBuffer.WriteFixed(VertexFlag.Color0, ref _color);

            //    vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);
            //}

            //for (var i = 0; i < _connections.Count; i++)
            //{
            //    vertexBuffer.WriteFixed(VertexFlag.Position0, new float[] { _connections[i].Point.X, _connections[i].Point.Y, _connections[i].Point.Z });

            //    vertexBuffer.WriteFixed(VertexFlag.Color0, new float[] { _connections[i].Color.R, _connections[i].Color.G, _connections[i].Color.B, _connections[i].Color.A });

            //    vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);
            //}
        }
    }

}