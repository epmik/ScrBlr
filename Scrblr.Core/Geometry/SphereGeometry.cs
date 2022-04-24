using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Scrblr.Core
{
    public enum SphereMode
    {
        IcoSphere = 0,
        // TODO implement UV-sphere
        UvSphere
    }

    public class SphereGeometry : AbstractGeometry<SphereGeometry>
    {
        public const float DefaultWidth = 1f;
        public const float DefaultHeight = 1f;
        public const float DefaultDepth = 1f;

        public SphereMode SphereMode { get; set; } = SphereMode.IcoSphere;

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

        /// <summary>
        /// default == 2
        /// </summary>
        public int _subdivisions = 2;

        private int _allocatedVertexCount;

        public SphereGeometry(SphereMode sphereMode = SphereMode.IcoSphere)
            : this(DefaultWidth, DefaultHeight, DefaultDepth)
        {

        }

        public SphereGeometry(float width, float height, float depth, SphereMode sphereMode = SphereMode.IcoSphere)
            : this(width, height, depth, Matrix4.Identity)
        {

        }

        public SphereGeometry(float width, float height, float depth, Matrix4 modelMatrix, SphereMode sphereMode = SphereMode.IcoSphere)
            : base(GeometryType.Triangles, 12, modelMatrix)
        {
            SphereMode = SphereMode.IcoSphere;
            VertexFlags = VertexFlag.Position0 | VertexFlag.Normal0;
            Subdivisions(_subdivisions);
        }

        public SphereGeometry Width(float w)
        {
            _width = w;

            return this;
        }

        public SphereGeometry Height(float h)
        {
            _height = h;

            return this;
        }

        public SphereGeometry Depth(float d)
        {
            _depth = d;

            return this;
        }

        public SphereGeometry Subdivisions(int subdivisions)
        {
            subdivisions = Math.Clamp(subdivisions, 1, 6); 

            _subdivisions = subdivisions;

            _vertexCount = TriangleCount() * 3;

            _allocatedVertexCount = 12;

            for (var i = 1; i < _subdivisions; i++)
            {
                _allocatedVertexCount += TriangleCount(i) * 3;
            }

            //switch (subdivisions)
            //{
            //    case 1:
            //        _allocatedVertexCount = 12;
            //        break;
            //    case 2:
            //        _allocatedVertexCount = 42;
            //        break;
            //    case 3:
            //        _allocatedVertexCount = 162;
            //        break;
            //    case 4:
            //        _allocatedVertexCount = 642;
            //        break;
            //    case 5:
            //        _allocatedVertexCount = 2562;
            //        break;
            //    default:
            //        _allocatedVertexCount = 10242;
            //        break;
            //}

            return this;
        }

        private static readonly float t = (float)((1.0 + Math.Sqrt(5.0)) * 0.5);

        private static readonly float[,] _defaultPoints =
        {
                { -1, t, 0 },
                { 1, t, 0 },
                { -1, -t, 0 },
                { 1, -t, 0 },

                { 0, -1, t },
                { 0, 1, t },
                { 0, -1, -t },
                { 0, 1, -t },

                { t, 0, -1 },
                { t, 0, 1 },
                { -t, 0, -1 },
                { -t, 0, 1 },
        };

        private static readonly int[,] _defaultTriangleIndices = new int[,]
        {
            { 0, 11, 5 },
            { 0, 5, 1 },
            { 0, 1, 7 },
            { 0, 7, 10 },
            { 0, 10, 11 },

            { 1, 5, 9 },
            { 5, 11, 4 },
            { 11, 10, 2 },
            { 10, 7, 6 },
            { 7, 1, 8 },

            { 3, 9, 4 },
            { 3, 4, 2 },
            { 3, 2, 6 },
            { 3, 6, 8 },
            { 3, 8, 9 },

            { 4, 9, 5 },
            { 2, 4, 11 },
            { 6, 2, 10 },
            { 8, 6, 7 },
            { 9, 8, 1 },
        };

        private int _triangleIndex;

        private int _pointIndex;

        private int TriangleCount()
        {
            return TriangleCount(_subdivisions);
        }

        private int TriangleCount(int subdivision)
        {
            var triangleCount = 20;

            for (var i = 1; i < subdivision; i++)
            {
                triangleCount *= 4;
            }

            return triangleCount;
        }

        private int[,] AllocateIndicesAndSetDefaults(bool fillDefaultValues = true)
        {
            var triangleIndices = new int[TriangleCount(), 3];

            if(fillDefaultValues)
            {
                for (var i = 0; i < _defaultTriangleIndices.GetLength(0); i++)
                {
                    triangleIndices[i, 0] = _defaultTriangleIndices[i, 0];
                    triangleIndices[i, 1] = _defaultTriangleIndices[i, 1];
                    triangleIndices[i, 2] = _defaultTriangleIndices[i, 2];

                    _triangleIndex++;
                }
            }

            return triangleIndices;
        }

        private float[,] AllocatePointsAndSetDefaults()
        {
            // see http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html

            var points = new float[_allocatedVertexCount, 3];

            for(var i = 0; i < _defaultPoints.GetLength(0); i++)
            {
                points[i, 0] = _defaultPoints[i, 0];
                points[i, 1] = _defaultPoints[i, 1];
                points[i, 2] = _defaultPoints[i, 2];

                _pointIndex++;
            }

            Normalize(ref points, 0, _defaultPoints.GetLength(0));

            return points;
        }

        private void Normalize(ref float [,] points, int index, int count)
        {
            for(var i = index; i < index + count; i++)
            {
                var factor = 0.5f * 1f / MathF.Sqrt(points[i, 0] * points[i, 0] + points[i, 1] * points[i, 1] + points[i, 2] * points[i, 2]);

                points[i, 0] *= factor;
                points[i, 1] *= factor;
                points[i, 2] *= factor;
            }
        }

        private void SubdividePoints(ref float[,] points, ref int[,] triangleIndices)
        {
            if(_subdivisions < 2)
            {
                return;
            }

            var tempTriangleIndices = AllocateIndicesAndSetDefaults(false);

            for (var subdivision = 1; subdivision < _subdivisions; subdivision++)
            {
                var triangleCount = _triangleIndex;
                _triangleIndex = 0;

                for (var i = 0; i < triangleCount; i++)
                {
                    tempTriangleIndices[i, 0] = triangleIndices[i, 0];
                    tempTriangleIndices[i, 1] = triangleIndices[i, 1];
                    tempTriangleIndices[i, 2] = triangleIndices[i, 2];
                }

                for (var i = 0; i < triangleCount; i++)
                {
                    int a = InsertMiddlePoint(ref points, tempTriangleIndices[i, 0], tempTriangleIndices[i, 1]);
                    int b = InsertMiddlePoint(ref points, tempTriangleIndices[i, 1], tempTriangleIndices[i, 2]);
                    int c = InsertMiddlePoint(ref points, tempTriangleIndices[i, 2], tempTriangleIndices[i, 0]);

                    triangleIndices[_triangleIndex, 0] = tempTriangleIndices[i, 0];
                    triangleIndices[_triangleIndex, 1] = a;
                    triangleIndices[_triangleIndex, 2] = c;

                    _triangleIndex++;

                    triangleIndices[_triangleIndex, 0] = tempTriangleIndices[i, 1];
                    triangleIndices[_triangleIndex, 1] = b;
                    triangleIndices[_triangleIndex, 2] = a;

                    _triangleIndex++;

                    triangleIndices[_triangleIndex, 0] = tempTriangleIndices[i, 2];
                    triangleIndices[_triangleIndex, 1] = c;
                    triangleIndices[_triangleIndex, 2] = b;

                    _triangleIndex++;

                    triangleIndices[_triangleIndex, 0] = a;
                    triangleIndices[_triangleIndex, 1] = b;
                    triangleIndices[_triangleIndex, 2] = c;

                    _triangleIndex++;
                }
            }
        }

        private int InsertMiddlePoint(ref float[,] points, int p1, int p2)
        {
            points[_pointIndex, 0] = (points[p1, 0] + points[p2, 0]) * 0.5f;
            points[_pointIndex, 1] = (points[p1, 1] + points[p2, 1]) * 0.5f;
            points[_pointIndex, 2] = (points[p1, 2] + points[p2, 2]) * 0.5f;

            Normalize(ref points, _pointIndex, 1);

            return _pointIndex++;
        }

        public override void WriteToVertexBuffer(VertexBuffer vertexBuffer)
        {
            var points = AllocatePointsAndSetDefaults();
            var indices = AllocateIndicesAndSetDefaults();
            SubdividePoints(ref points, ref indices);
            var normalIndex = 0;
            var uvIndex = 0;

            for (var i = 0; i < indices.GetLength(0); i++)
            {
                normalIndex = (int)(i / 6.0);
                uvIndex = i % 6;

                for(var j = 0; j < 3; j++)
                {
                    vertexBuffer.WriteFixed(VertexFlag.Position0, new float[] { points[indices[i, j], 0], points[indices[i, j], 1], points[indices[i, j], 2] });

                    //vertexBuffer.WriteFixed(VertexFlag.Normal0, new float[] { _normals[normalIndex, 0], _normals[normalIndex, 1], _normals[normalIndex, 2] });

                    vertexBuffer.WriteFixed(VertexFlag.Color0, ref _color);

                    //vertexBuffer.WriteFixed(VertexFlag.Uv0, new float[] { _uvs[uvIndex, 0], _uvs[uvIndex, 1] });

                    //vertexBuffer.WriteFixed(VertexFlag.Uv1, new float[] { _uvs[uvIndex, 0], _uvs[uvIndex, 1] });

                    vertexBuffer.WriteDefaultValuesUntil(VertexFlag.Position0);
                }
            }
        }
    }
}