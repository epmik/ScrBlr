using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public class RectangleGeometry : AbstractGeometry<RectangleGeometry>
    {
        /// <summary>
        /// default == 1f
        /// </summary>
        public float _width = 1f;

        /// <summary>
        /// default == 1f
        /// </summary>
        public float _height = 1f;

        public RectangleGeometry()
            : base(GeometryType.TriangleStrip, 4, Matrix4.Identity)
        {

        }

        public RectangleGeometry(Matrix4 modelMatrix)
            : base(GeometryType.TriangleStrip, 4, modelMatrix)
        {

        }

        public RectangleGeometry Width(float w)
        {
            _width = w;

            return this;
        }

        public RectangleGeometry Height(float h)
        {
            _height = h;

            return this;
        }

        // OpenGL, unlike other graphics API's, has its texture coordinate origin in the bottom-left corner of a texture. Other API's have this origin in the top left.
        private static readonly float[,] _defaultUvs = new float[,]
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
        // OpenGL, unlike other graphics API's, has its texture coordinate origin in the bottom-left corner of a texture. Other API's have this origin in the top left.
        //private static readonly float[][] _defaultUvs = new float[][]
        //    {
        //            // top left
        //            new float[] { 0f, 0f },
        //            // bottom left
        //            new float[] { 0, 1f },
        //            // top right
        //            new float[] { 1f, 0f },
        //            // bottom right
        //            new float[] { 1f, 1f },
        //    };

        private float[,] CalculatePoints()
        {
            var hw = _width * 0.5f;
            var hh = _height * 0.5f;

            // ccw triangle order
            return new float[,]
            {
                    // top left
                    { _position.X - hw, _position.Y + hh, 0f },
                    // bottom left
                    { _position.X - hw, _position.Y - hh, 0f },
                    // top right
                    { _position.X + hw, _position.Y + hh, 0f },
                    // bottom right
                    { _position.X + hw, _position.Y - hh, 0f },
            };
        }

        public override void WriteToVertexBuffer(VertexBuffer vertexBuffer)
        {
            var points = CalculatePoints();

            for (var i = 0; i < 4; i++)
            {
                vertexBuffer.WriteFixed(VertexFlag.Position0, new float[] { points[i, 0], points[i, 1], points[i, 2] });

                vertexBuffer.WriteFixed(VertexFlag.Normal0, DefaultNormal);

                vertexBuffer.WriteFixed(VertexFlag.Color0, ref _color);

                vertexBuffer.WriteFixed(VertexFlag.Uv0, new float[] { _defaultUvs[i, 0], _defaultUvs[i, 1] });

                vertexBuffer.WriteFixed(VertexFlag.Uv1, new float[] { _defaultUvs[i, 0], _defaultUvs[i, 1] });

                vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);
            }
        }
    }

}