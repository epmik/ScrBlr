using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public class CubeGeometry : AbstractGeometry<CubeGeometry>
    {
        public const float DefaultWidth = 1f;
        public const float DefaultHeight = 1f;
        public const float DefaultDepth = 1f;

        /// <summary>
        /// default == 1f
        /// </summary>
        public float _width = 1f;

        /// <summary>
        /// default == 1f
        /// </summary>
        public float _height = 1f;

        /// <summary>
        /// default == 1f
        /// </summary>
        public float _depth = 1f;

        public CubeGeometry()
            : this(DefaultWidth, DefaultHeight, DefaultDepth)
        {

        }

        public CubeGeometry(float width, float height, float depth)
            : this(width, height, depth, Matrix4.Identity)
        {

        }

        public CubeGeometry(float width, float height, float depth, Matrix4 modelMatrix)
            : base(GeometryType.Triangles, 36, modelMatrix)
        {
            VertexFlags = VertexFlag.Position0 | VertexFlag.Normal0;
        }

        public CubeGeometry Width(float w)
        {
            _width = w;

            return this;
        }

        public CubeGeometry Height(float h)
        {
            _height = h;

            return this;
        }

        public CubeGeometry Depth(float d)
        {
            _depth = d;

            return this;
        }

        private static readonly float[,] _uvs = new float[,]
        {
                // top left
                { 0f, 1f },

                // bottom left
                { 0, 0f },

                // top right
                { 1f, 1f },

                // top right
                { 1f, 1f },

                // bottom left
                { 0, 0f },

                // bottom right
                { 1f, 0f },
        };

        private static readonly float[,] _normals = new float[,]
        {
            // front face
            { 0f, 0f, 1f },
            // back face
            { 0f, 0f, -1f },
            // right face
            { 1f, 0f, 0f },
            // left face
            { -1f, 0f, 0f },
            // top face
            { 0f, 1f, 0f },
            // bottom face
            { 0f, -1f, 0f },
        };

        private static readonly int[] _pointIndices = new int[]
        {
            // front face
            0, 1, 2, 2, 1, 3,
            // back face
            4, 5, 6, 6, 5, 7,
            // right face
            2, 3, 4, 4, 3, 5,
            // left face
            6, 7, 0, 0, 7, 1,
            // top face
            6, 0, 4, 4, 0, 2,
            // bottom face
            1, 7, 3, 3, 7, 5,
        };

        private float[,] CalculatePoints()
        {
            var hw = _width * 0.5f;
            var hh = _height * 0.5f;
            var hd = _depth * 0.5f;

            return new float[,]
            {
                // front
                // 0 top left
                { _position.X - hw, _position.Y + hh, _position.Y + hd },
                // 1 bottom left
                { _position.X - hw, _position.Y - hh, _position.Y + hd },
                // 2 top right
                { _position.X + hw, _position.Y + hh, _position.Y + hd },
                // 3 bottom right
                { _position.X + hw, _position.Y - hh, _position.Y + hd },

                // back
                // 4 top right
                { _position.X + hw, _position.Y + hh, _position.Y - hd },
                // 5 bottom right
                { _position.X + hw, _position.Y - hh, _position.Y - hd },
                // 6 top left
                { _position.X - hw, _position.Y + hh, _position.Y - hd },
                // 7 bottom left
                { _position.X - hw, _position.Y - hh, _position.Y - hd },
            };
        }


        public override void WriteToVertexBuffer(VertexBuffer vertexBuffer)
        {
            var points = CalculatePoints();
            var normalIndex = 0;
            var uvIndex = 0;

            for (var i = 0; i < _pointIndices.Length; i++)
            {
                normalIndex = (int)(i / 6.0);
                uvIndex = i % 6;

                vertexBuffer.WriteFixed(VertexFlag.Position0, new float[] { points[_pointIndices[i], 0], points[_pointIndices[i], 1], points[_pointIndices[i], 2] });

                vertexBuffer.WriteFixed(VertexFlag.Normal0, new float[] { _normals[normalIndex, 0], _normals[normalIndex, 1], _normals[normalIndex, 2] });

                vertexBuffer.WriteFixed(VertexFlag.Color0, ref _color);

                vertexBuffer.WriteFixed(VertexFlag.Uv0, new float[] { _uvs[uvIndex, 0], _uvs[uvIndex, 1] });

                vertexBuffer.WriteFixed(VertexFlag.Uv1, new float[] { _uvs[uvIndex, 0], _uvs[uvIndex, 1] });

                vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);
            }
        }
    }

}